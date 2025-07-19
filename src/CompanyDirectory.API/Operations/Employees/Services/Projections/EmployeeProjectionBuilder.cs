using CompanyDirectory.API.Infrastructure.Mongo;
using CompanyDirectory.API.Operations.Companies.Services.Projections;
using CompanyDirectory.API.Operations.Employees.Services.Grains.EmployeeGrain;
using CompanyDirectory.API.Services.Projections.Registry;
using MongoDB.Driver;

namespace CompanyDirectory.API.Operations.Employees.Services.Projections;

[GrainType("employee-projection")]
[ProjectionsRegistryDiscovery(targetStateType: typeof(EmployeeGrainState), grainType: "employee-projection")]
public class EmployeeProjectionBuilder : IProjectionBuilderGrain
{
    private readonly IMongoClient _mongoClient;
    private readonly IEmployeeProjectionStore _store;

    public EmployeeProjectionBuilder(IMongoClient mongoClient, IEmployeeProjectionStore store)
    {
        _mongoClient = mongoClient;
        _store = store;
    }

    public async Task<ConstructProjectionResult> Construct(ConstructProjectionRequest request)
    {
        var result = request.State switch
        {
            EmployeeGrainState s => await Handle(s),
            _ => new ConstructProjectionResult { ValidationErrors = new() { { "state", "Not support state type" } } },
        };

        return result;
    }

    private async Task<ConstructProjectionResult> Handle(EmployeeGrainState state)
    {
        var collection = _mongoClient.GetCollection<CompanyProjection>();

        var companyProjection = state.CompanyId is not null
            ? collection.Find(s => s.Id == state.CompanyId).FirstOrDefault()
            : null;

        var projection = new EmployeeProjection
        {
            Id = state.Id,
            Name = new EmployeeNameProjection
            {
                FirstName = state.FirstName,
                LastName = state.LastName
            },
            Department = state.Department,
            Salary = state.Salary,
            Company = companyProjection is not null
                            ? new EmployeeCompanyProjection
                            {
                                Id = companyProjection.Id,
                                CompanyName = companyProjection.CompanyName,
                                Industry = companyProjection.Industry,
                            }
                            : null
        };

        await _store.UpsertAsync(projection, CancellationToken.None);

        return new ConstructProjectionResult
        {
            Status = ConstructProjectionResultStatus.Success,
            ValidationErrors = []
        };
    }
}
