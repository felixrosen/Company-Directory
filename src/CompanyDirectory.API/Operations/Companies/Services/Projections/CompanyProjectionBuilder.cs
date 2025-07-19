using CompanyDirectory.API.Operations.Companies.Services.Grains.CompanyGrain;
using CompanyDirectory.API.Services.Projections;
using CompanyDirectory.API.Services.Projections.Registry;
using MongoDB.Driver;

namespace CompanyDirectory.API.Operations.Companies.Services.Projections;

[GrainType("company-projection")]
[ProjectionsRegistryDiscovery(targetStateType: typeof(CompanyGrainState), grainType: "company-projection")]
public class CompanyProjectionBuilder : IProjectionBuilderGrain
{
    private readonly ICompanyProjectionStore _store;
    private readonly IMongoClient _mongoClient;

    public CompanyProjectionBuilder(ICompanyProjectionStore store, IMongoClient mongoClient)
    {
        _store = store;
        _mongoClient = mongoClient;
    }

    public async Task<ConstructProjectionResult> Construct(ConstructProjectionRequest request)
    {
        var result = request.State switch
        {
            CompanyGrainState s => await Handle(s),
            _ => new ConstructProjectionResult { ValidationErrors = new() { { "state", "Not support state type" } } },
        };

        return result;
    }

    public async Task<ConstructProjectionResult> Handle(CompanyGrainState state)
    {
        var projection = ConstructProjection(state);

        await _store.UpsertAsync(projection, CancellationToken.None);

        return new ConstructProjectionResult
        {
            Status = ConstructProjectionResultStatus.Success,
        };
    }

    private static CompanyProjection ConstructProjection(CompanyGrainState state)
    {
        return new CompanyProjection
        {
            Id = state.Id,

            CompanyName = state.Info.CompanyName,
            CeoName = state.Info.CeoName,
            FoundedDate = state.Info.FoundedDate,
            Industry = state.Info.Industry,

            Metrics = new CompanyProjectionMetric
            {
                EmployeeCount = null,
                Revenue = state.Metrics.Revenue,
            },

            Address = new CompanyProjectionAddress
            {
                HqAddress = state.Addresses.HqAddress,
                HqCity = state.Addresses.HqCity,
                HqCountry = state.Addresses.HqCountry,
                HqState = state.Addresses.HqState,
            },

            UpdatedDateTime = state.UpdateDateTime,
            State = state.StateType switch
            {
                CompanyGrainStateType.Created => ProjectionStateType.Created,
                CompanyGrainStateType.Updated => ProjectionStateType.Updated,
                CompanyGrainStateType.Deleted => ProjectionStateType.Deleted,
                _ => throw new NotImplementedException("Unknown state type")
            }
        };
    }
}
