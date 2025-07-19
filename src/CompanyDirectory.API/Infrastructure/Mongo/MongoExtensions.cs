using CompanyDirectory.API.Services.Projections;
using MongoDB.Driver;

namespace CompanyDirectory.API.Infrastructure.Mongo;

public interface IMongoProjection
{
    abstract static string CollectionName { get; }
    ProjectionStateType State { get; set; }
}

public static class MongoCollectionExtensions
{
    public static IMongoCollection<T> GetCollection<T>(this IMongoClient client) where T : IMongoProjection
    {
        return client.GetDatabase("company-directory").GetCollection<T>(T.CollectionName);
    }

    public static IMongoDatabase GetDatabase(this IMongoClient client, string databaseName = "company-directory")
    {
        return client.GetDatabase(databaseName);
    }
}
