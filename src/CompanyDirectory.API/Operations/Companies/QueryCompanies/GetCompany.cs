using CompanyDirectory.API.Infrastructure.Mongo;
using CompanyDirectory.API.Operations.Companies.Services.Grains.CompanyGrain;
using CompanyDirectory.API.Operations.Companies.Services.Projections;
using CompanyDirectory.API.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace CompanyDirectory.API.Operations.Companies.QueryCompanies;

public class GetCompany : IEndpoint
{
    public string Route => "/companies";

    public void Bootstrap(WebApplication app, string baseRoute)
    {
        app.MapGet($"{baseRoute}{Route}", async (string Id, [FromServices] IMongoClient mongoClient) =>
        {
            var collection = mongoClient.GetCollection<CompanyProjection>();
            var company = await collection.Find(x => x.Id == Id).FirstOrDefaultAsync();

            if (company is not null)
            {
                return Results.Ok(company);
            }

            return Results.NotFound($"Company with id {Id} not found");
        })
        .Produces<CompanyProjection>(StatusCodes.Status200OK)
        .WithTags("Companies");

        app.MapGet($"{baseRoute}{Route}/state", async (string Id, [FromServices] IClusterClient cluster) =>
        {
            if (Guid.TryParse(Id, out var id))
            {
                var grain = cluster.GetGrain<ICompanyGrain>(id);
                var state = await grain.GetState();

                return Results.Ok(state);
            }

            return Results.BadRequest("Invalid id");
        })
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Get Customers",
            Description = "Returns a list of all customers.",
            OperationId = "GetCustomers",
        });
    }
}