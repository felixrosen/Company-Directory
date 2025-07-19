using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using CompanyDirectory.API.Services.Projections.Registry;

namespace CompanyDirectory.API.Services.Projections;

public interface IProjectionBuilderQueue
{
    IAsyncEnumerable<List<ConstructProjectionRequest>> Dequeue(CancellationToken cancellationToken);
    ValueTask Enqueue(ConstructProjectionRequest item);
}

public class ProjectionBuilderQueue : IProjectionBuilderQueue
{
    private readonly Channel<ConstructProjectionRequest> _channel;
    private readonly TimeSpan? _maxWait;

    public ProjectionBuilderQueue(TimeSpan? maxWait = null)
    {
        _channel = Channel.CreateUnbounded<ConstructProjectionRequest>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
        });
        _maxWait = maxWait;
    }

    public async ValueTask Enqueue(ConstructProjectionRequest item)
    {
        await _channel.Writer.WriteAsync(item);
    }

    public async IAsyncEnumerable<List<ConstructProjectionRequest>> Dequeue([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var batchSize = 100;
        var maxWait = _maxWait ?? TimeSpan.FromSeconds(10);

        var batch = new List<ConstructProjectionRequest>(batchSize);
        var delayCts = new CancellationTokenSource();
        Task? delayTask = null;

        while (await _channel.Reader.WaitToReadAsync(cancellationToken))
        {
            while (_channel.Reader.TryRead(out var item))
            {
                if (batch.Count == 0)
                {
                    // Start timer when first item is added
                    delayCts = new CancellationTokenSource();
                    delayTask = Task.Delay(maxWait, delayCts.Token);
                }

                batch.Add(item);

                if (batch.Count >= batchSize)
                {
                    delayCts.Cancel(); // cancel timeout if we hit batch size
                    yield return batch;

                    batch = new List<ConstructProjectionRequest>(batchSize);
                    delayTask = null;
                }
            }

            if (batch.Count > 0 && delayTask != null)
            {
                var completedTask = await Task.WhenAny(_channel.Reader.WaitToReadAsync(cancellationToken).AsTask(), delayTask);
                if (completedTask == delayTask)
                {
                    yield return batch;
                    batch = new List<ConstructProjectionRequest>(batchSize);
                    delayTask = null;
                }
            }
        }

        // Final flush after channel completes
        if (batch.Count > 0)
        {
            yield return batch;
        }
    }
}

public class ProjectionBuilderQueueService : BackgroundService
{
    private readonly IProjectionBuilderQueue _queue;
    private readonly ILogger<ProjectionBuilderQueueService> _logger;
    private readonly IGrainFactory _grainFactory;

    public ProjectionBuilderQueueService(IProjectionBuilderQueue queue,
                                         ILogger<ProjectionBuilderQueueService> logger,
                                         IGrainFactory grainFactory)
    {
        _queue = queue;
        _logger = logger;
        _grainFactory = grainFactory;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{Name} is running", nameof(ProjectionBuilderQueueService));

        return ProcessQueue(stoppingToken);
    }

    private async Task ProcessQueue(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Dequeued item");

                await foreach (var constructProjectionRequests in _queue.Dequeue(cancellationToken))
                {
                    var processingTasks = new List<Task>();
                    foreach (var constructProjectionRequest in constructProjectionRequests)
                    {
                        foreach (var (name, fullName) in constructProjectionRequest.Projections)
                        {
                            var grain = _grainFactory.GetGrain<IProjectionBuilderGrain>(constructProjectionRequest.Id, name, fullName);
                            var processingTask = grain.Construct(constructProjectionRequest);

                            processingTasks.Add(processingTask);
                        }
                    }

                    var sw = Stopwatch.StartNew();

                    await Task.WhenAll(processingTasks).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

                    sw.Stop();
                    _logger.LogInformation("Processed {Count} projection requests in {Elapsed}",
                                           constructProjectionRequests.Count,
                                           sw.Elapsed);
                }
            }
            catch (OperationCanceledException) { }
            catch (InvalidOperationException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not process queue item {Message}", ex.Message);
            }
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{nameof(ProjectionBuilderQueueService)} is stopping.");

        await base.StopAsync(stoppingToken);
    }
}
