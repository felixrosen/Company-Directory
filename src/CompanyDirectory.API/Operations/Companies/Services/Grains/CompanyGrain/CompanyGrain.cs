using CompanyDirectory.API.Infrastructure.Orleans;
using CompanyDirectory.API.Infrastructure.Orleans.Types;
using CompanyDirectory.API.Operations.Companies.Services.Grains.CompanyGrain.Types;
using CompanyDirectory.API.Operations.Companies.Services.Projections;
using CompanyDirectory.API.Services.Projections;
using CompanyDirectory.API.Services.Projections.Registry;

namespace CompanyDirectory.API.Operations.Companies.Services.Grains.CompanyGrain;

[Alias("ICompanyGrain")]
public interface ICompanyGrain : IGrainWithGuidKey
{
    [Alias("Handle")]
    public Task<CompanyGrainResponse> Handle<T>(GrainRequest<T> request) where T : IGrainRequest;

    [Alias("ReplayEvents")]
    public Task<CompanyGrainResponse> ReplayEvents();

    [Alias("GetState")]
    public Task<CompanyGrainResponse> GetState();
}

[GenerateSerializer]
[Alias("CompanyGrainResponse")]
public class CompanyGrainResponse
{
    [Id(0)]
    public required CompanyGrainState? State { get; set; }
}

public class CompanyGrain : GrainBase<CompanyGrain, PersistedState>, ICompanyGrain
{
    private readonly IProjectionBuilderQueue _projectionQueue;
    private readonly ICompanyProjectionStore _projectionStore;
    private readonly IProjectionsRegistryServiceClient _registryClient;
    private CompanyGrainState? _state;

    public CompanyGrain(ILogger<CompanyGrain> logger,
                        [PersistentState("companies")] IPersistentState<PersistedState> persistentState,
                        IProjectionBuilderQueue projectionQueue,
                        ICompanyProjectionStore projectionStore,
                        IProjectionsRegistryServiceClient registryClient) : base(logger, persistentState)
    {
        _projectionQueue = projectionQueue;
        _projectionStore = projectionStore;
        _registryClient = registryClient;
    }


    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        base.OnActivateAsync(cancellationToken);

        _state = CompanyGrainState.Apply(_state, _persistentState.State.Events);

        return Task.CompletedTask;
    }

    public async Task<CompanyGrainResponse> Handle<T>(GrainRequest<T> request) where T : IGrainRequest
    {
        var events = request.Request switch
        {
            ImportCompanyRequest i => Handle(i),
            DeleteCompanyRequest d => Handle(d),
            _ => HandleUnknownRequest(request),
        };

        _persistentState.State.Requests.Add(request.Request);
        _persistentState.State.Events.AddRange(events);

        await _persistentState.WriteStateAsync();

        _state = CompanyGrainState.Apply(_state, events);

        await AddProjectionToQueue(events);

        return new CompanyGrainResponse { State = _state };
    }

    public async Task<CompanyGrainResponse> ReplayEvents()
    {
        if (_state is null)
            return new CompanyGrainResponse { State = null };

        var events = _persistentState.State.Events;

        _state = CompanyGrainState.Apply(_state, events);

        await AddProjectionToQueue(events);

        return new CompanyGrainResponse { State = _state };
    }

    public Task<CompanyGrainResponse> GetState()
    {
        var state = CompanyGrainState.Apply(null, _persistentState.State.Events);

        return Task.FromResult(new CompanyGrainResponse { State = state });
    }

    private List<IEvent> Handle(ImportCompanyRequest request)
    {
        // Validate
        if (_state is null)
            return [CompanyCreated.Construct(request)];

        var events = new List<IEvent>();

        /* We have already imported the company, hence, update */

        if (_state.Addresses.HasChanged(request.HqAddress, request.HqCity, request.HqState, request.HqCountry))
            events.Add(CompanyAddressUpdated.Construct(request));

        if (_state.Info.HasChanged(request.CompanyName, request.CeoName, request.Industry, request.FoundedDate))
            events.Add(CompanyInfoUpdated.Construct(request));

        if (_state.Metrics.HasChanged(request.Revenue, request.EmployeeCount))
            events.Add(CompanyMetricsUpdated.Construct(request));

        return events;
    }

    private List<IEvent> Handle(DeleteCompanyRequest _)
    {
        if (_state is null)
            return [];

        if (_state.StateType == CompanyGrainStateType.Deleted)
            return [];

        return [CompanyDeleted.Construct()];
    }

    private async Task AddProjectionToQueue(List<IEvent> events)
    {
        if (events is { Count: > 0 } && _state is { })
        {
            var projectionsResponse = await _registryClient.GetProjections(new GetRegisteredProjectionsFromTypeRequest
            {
                Type = _state.GetType()
            });

            await _projectionQueue.Enqueue(new ConstructProjectionRequest
            {
                Id = this.GetPrimaryKey(),
                StateType = _state.GetType(),
                State = _state,
                Events = events,
                Projections = projectionsResponse.Projections,
            });
        }
    }
}
