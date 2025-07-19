using CompanyDirectory.API.Infrastructure.Mongo;
using MongoDB.Driver;

namespace CompanyDirectory.API.Operations.Companies.Services.Projections;

public interface ICompanyProjectionStore
{
    Task UpsertAsync(CompanyProjection projection, CancellationToken cancellation);
}

public class CompanyProjectionStore : ICompanyProjectionStore
{
    private readonly IMongoClient _mongoClient;

    public CompanyProjectionStore(IMongoClient mongoClient)
    {
        _mongoClient = mongoClient;
    }

    public async Task UpsertAsync(CompanyProjection projection, CancellationToken cancellationToken)
    {
        var collection = _mongoClient.GetCollection<CompanyProjection>();
        await collection.ReplaceOneAsync(s => s.Id == projection.Id,
                                              projection,
                                              new ReplaceOptions { IsUpsert = true }, cancellationToken: cancellationToken);
    }
}
