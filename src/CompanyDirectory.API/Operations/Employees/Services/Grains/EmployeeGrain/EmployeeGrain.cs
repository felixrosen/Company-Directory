using CompanyDirectory.API.Infrastructure.Orleans;
using CompanyDirectory.API.Infrastructure.Orleans.Types;
using CompanyDirectory.API.Operations.Employees.Services.Grains.EmployeeGrain.Types;
using CompanyDirectory.API.Services.Projections;
using CompanyDirectory.API.Services.Projections.Registry;

namespace CompanyDirectory.API.Operations.Employees.Services.Grains.EmployeeGrain;

[Alias("IEmployeeGrain")]
public interface IEmployeeGrain : IGrainWithGuidKey
{
    [Alias("Handle")]
    Task<EmployeeGrainResponse> Handle<T>(GrainRequest<T> request) where T : IGrainRequest;

    [Alias("ReplayEvents")]
    Task<EmployeeGrainResponse> ReplayEvents();

    [Alias("GetState")]
    Task<EmployeeGrainResponse> GetState();
}

public class EmployeeGrain : GrainBase<EmployeeGrain, PersistedState>, IEmployeeGrain
{
    private EmployeeGrainState? _state;
    private readonly IProjectionBuilderQueue _projectionQueue;
    private readonly IProjectionsRegistryServiceClient _registryClient;

    public EmployeeGrain(ILogger<EmployeeGrain> logger,
                       [PersistentState("employees")] IPersistentState<PersistedState> persistentState,
                       IProjectionBuilderQueue projectionQueue,
                       IProjectionsRegistryServiceClient registryClient) : base(logger, persistentState)
    {
        _projectionQueue = projectionQueue;
        _registryClient = registryClient;
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        base.OnActivateAsync(cancellationToken);

        _state = EmployeeGrainState.Apply(_state, _persistentState.State.Events);
        return Task.CompletedTask;
    }

    public async Task<EmployeeGrainResponse> Handle<T>(GrainRequest<T> request) where T : IGrainRequest
    {
        var events = request.Request switch
        {
            ImportEmployeeRequest i => Handle(i),
            _ => throw new NotImplementedException("Request type not implemented")
        };

        _persistentState.State.Requests.Add(request.Request);
        _persistentState.State.Events.AddRange(events);

        await _persistentState.WriteStateAsync();

        _state = EmployeeGrainState.Apply(_state, events);

        await AddProjectionToQueue(events);

        return new EmployeeGrainResponse { State = _state };
    }

    public async Task<EmployeeGrainResponse> ReplayEvents()
    {
        if (_state is null)
            return new EmployeeGrainResponse { State = null };

        var events = _persistentState.State.Events;

        _state = EmployeeGrainState.Apply(_state, events);

        await AddProjectionToQueue(events);

        return new EmployeeGrainResponse { State = _state };
    }

    public Task<EmployeeGrainResponse> GetState()
    {
        return Task.FromResult(new EmployeeGrainResponse { State = _state });
    }

    private List<IEvent> Handle(ImportEmployeeRequest request)
    {
        if (_state is null)
            return [EmployeeCreated.Construct(request)];

        var events = new List<IEvent>();

        if (_state.FirstName != request.FirstName || _state.LastName != request.LastName)
            events.Add(EmployeeNameUpdated.Construct(request));

        if (_state.Salary != request.Salary)
            events.Add(EmployeeSalaryUpdated.Construct(request));

        if (_state.Department != request.Department)
            events.Add(EmployeeDepartmentUpdated.Construct(request));

        return events;
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
                Projections = projectionsResponse.Projections
            });
        }
    }
}
