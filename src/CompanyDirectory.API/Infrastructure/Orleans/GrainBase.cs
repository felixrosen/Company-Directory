using CompanyDirectory.API.Infrastructure.Orleans.Types;
using CompanyDirectory.API.Operations.Companies.Services.Grains.CompanyGrain;

namespace CompanyDirectory.API.Infrastructure.Orleans;

public class GrainBase<TType, TState> : Grain where TState : IPersistedState
{
    protected readonly ILogger<TType> _logger;
    protected readonly IPersistentState<TState> _persistentState;

    public GrainBase(ILogger<TType> logger,
                     IPersistentState<TState> persistentState)
    {
        _logger = logger;
        _persistentState = persistentState;
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        //_logger.LogInformation("Base grain activated");

        _persistentState.State.Id ??= this.GetPrimaryKey().ToString();

        return base.OnActivateAsync(cancellationToken);
    }

    public List<IEvent> HandleUnknownRequest<T>(GrainRequest<T> _) where T : IGrainRequest
    {
        _logger.LogError("Unknown request type {RequestType} for grain {GrainType} with key {GrainKey}", typeof(T).Name,
                         nameof(CompanyGrain), this.GetPrimaryKeyString());
        throw new NotImplementedException("Unknown request type");
    }
}
