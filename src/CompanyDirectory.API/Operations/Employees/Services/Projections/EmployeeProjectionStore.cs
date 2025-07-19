using CompanyDirectory.API.Infrastructure.Mongo;
using MongoDB.Driver;

namespace CompanyDirectory.API.Operations.Employees.Services.Projections;

public interface IEmployeeProjectionStore
{
    Task UpsertAsync(EmployeeProjection projection, CancellationToken cancellation);
}

public class EmployeeProjectionStore : IEmployeeProjectionStore
{
    private readonly IMongoClient _mongoClient;

    public EmployeeProjectionStore(IMongoClient mongoClient)
    {
        _mongoClient = mongoClient;
    }

    public async Task UpsertAsync(EmployeeProjection projection, CancellationToken cancellationToken)
    {
        var collection = _mongoClient.GetCollection<EmployeeProjection>();
        await collection.ReplaceOneAsync(s => s.Id == projection.Id,
                                              projection,
                                              new ReplaceOptions { IsUpsert = true }, cancellationToken: cancellationToken);
    }
}
