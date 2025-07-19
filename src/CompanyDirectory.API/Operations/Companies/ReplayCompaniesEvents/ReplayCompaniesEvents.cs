using CompanyDirectory.API.Infrastructure.Mongo;
using CompanyDirectory.API.Infrastructure.Mongo.Queries;
using CompanyDirectory.API.Operations.Companies.Services.Grains.CompanyGrain;
using CompanyDirectory.API.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CompanyDirectory.API.Operations.Companies.ReplayCompaniesEvents;

public class ReplayCompaniesEvents : IEndpoint
{
    public string Route => "/companies";

    public void Bootstrap(WebApplication app, string baseRoute)
    {
        app.MapPost($"{baseRoute}{Route}/replay-events", async ([FromServices] IMongoQueryHandler<GetGrainsIdQueryHandler.Query, GetGrainsIdQueryHandler.Result> queryHandler,
                                                                [FromServices] IClusterClient cluster,
                                                                [FromServices] IMongoClient client) =>
        {
            var result = await queryHandler.Handle(new GetGrainsIdQueryHandler.Query
            {
                Collection = client.GetDatabase().GetCollection<BsonDocument>("grain-state-companies")
            });

            foreach (var batch in result.GrainsId.Chunk(150))
            {
                var tasks = new List<Task>();

                foreach (var id in batch)
                {
                    var grain = cluster.GetGrain<ICompanyGrain>(id);
                    var task = grain.ReplayEvents();
                    tasks.Add(task);
                }

                await Task.WhenAll(tasks).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
            }

            return Results.Ok();
        })
        .WithTags("Companies");
    }
}
