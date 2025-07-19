using MongoDB.Bson;
using MongoDB.Driver;

namespace CompanyDirectory.API.Infrastructure.Mongo.Queries;

public class GetGrainsIdQueryHandler : IMongoQueryHandler<GetGrainsIdQueryHandler.Query, GetGrainsIdQueryHandler.Result>
{
    public class Query
    {
        public required IMongoCollection<BsonDocument> Collection { get; init; }
    }

    public class Result
    {
        public required List<Guid> GrainsId { get; init; }
    }

    public async Task<Result> Handle(Query query)
    {
        var filter = new BsonDocument();
        var projection = new BsonDocument { { "_doc._id", 1 } };

        var result = await query.Collection.Find(filter).Project(projection).ToListAsync();

        var grainsId = result.Select(x => Guid.Parse(x["_doc"]["_id"].AsString)).ToList();

        return new Result
        {
            GrainsId = grainsId
        };
    }
}
