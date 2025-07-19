using CompanyDirectory.API.Infrastructure.Mongo;
using CompanyDirectory.API.Infrastructure.Orleans.Types;
using CompanyDirectory.API.Operations.Employees.Services.Grains.EmployeeGrain;
using CompanyDirectory.API.Operations.Employees.Services.Grains.EmployeeGrain.Types;
using CompanyDirectory.API.Services.Projections;
using CompanyDirectory.API.Services.Projections.Registry;
using MongoDB.Driver;

namespace CompanyDirectory.API.Operations.Companies.Services.Projections;

[GrainType("company-employees-projection")]
[ProjectionsRegistryDiscovery(targetStateType: typeof(EmployeeGrainState), grainType: "company-employees-projection")]
public class CompanyEmployeesProjectionBuilder : IProjectionBuilderGrain
{
    private readonly IClusterClient _cluster;
    private readonly IMongoClient _mongoClient;
    private readonly ICompanyProjectionStore _store;

    public CompanyEmployeesProjectionBuilder(IClusterClient cluster, IMongoClient mongoClient, ICompanyProjectionStore store)
    {
        _cluster = cluster;
        _mongoClient = mongoClient;
        _store = store;
    }

    public async Task<ConstructProjectionResult> Construct(ConstructProjectionRequest request)
    {
        var result = request.State switch
        {
            EmployeeGrainState s => await Handle(s, request.Events),
            _ => new ConstructProjectionResult { ValidationErrors = new() { { "state", "Not support state type" } } },
        };

        return result;
    }

    public async Task<ConstructProjectionResult> Handle(EmployeeGrainState state, List<IEvent> events)
    {
        var companyProjectionCollection = _mongoClient.GetCollection<CompanyProjection>();
        var companyProjection = state.CompanyId is not null
            ? await companyProjectionCollection.Find(s => s.Id == state.CompanyId).FirstOrDefaultAsync()
            : null;

        if (companyProjection is null)
            return ConstructProjectionResult.ConstructSuccess();

        var lastEvent = events.LastOrDefault();

        if (lastEvent is null)
            return ConstructProjectionResult.ConstructSuccess();

        if (lastEvent is not EmployeeCreated)
            return ConstructProjectionResult.ConstructSuccess();

        companyProjection.State = ProjectionStateType.Updated;
        companyProjection.Employees.Add(new CompanyProjectionEmployee
        {
            Id = state.Id,

            FirstName = state.FirstName,
            LastName = state.LastName,

            Department = state.Department,
            Salary = state.Salary,
        });

        companyProjection.Metrics.EmployeeCount = companyProjection.Employees.Count;

        await _store.UpsertAsync(companyProjection, CancellationToken.None);

        return new ConstructProjectionResult
        {
            Status = ConstructProjectionResultStatus.Success,
        };
    }
}
