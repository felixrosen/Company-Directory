using CompanyDirectory.API.Infrastructure.Mongo;
using CompanyDirectory.API.Operations.Employees.Services.Grains.EmployeeGrain;
using CompanyDirectory.API.Operations.Employees.Services.Grains.EmployeeGrain.Types;
using CompanyDirectory.API.Operations.Employees.Services.Projections;
using CompanyDirectory.API.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace CompanyDirectory.API.Operations.Employees.QueryEmployees;

public class GetEmployees : IEndpoint
{
    public string Route => "/employees";

    public void Bootstrap(WebApplication app, string baseRoute)
    {
        app.MapGet($"{baseRoute}{Route}", async (string id,
                                                 [FromServices] IMongoClient mongoClient,
                                                 CancellationToken cancellationToken) =>
        {
            var employee = await mongoClient.GetCollection<EmployeeProjection>()
                                          .Find(x => x.Id == id)
                                          .FirstOrDefaultAsync(cancellationToken);

            if (employee is not null)
            {
                return Results.Ok(employee);
            }

            return Results.NotFound($"Employee with id {id} not found");
        })
        .WithTags("Employees")
        .Produces<EmployeeProjection>(StatusCodes.Status200OK);

        app.MapGet($"{baseRoute}{Route}/state", async (Guid id,
                                                       [FromServices] IGrainFactory grainFactory,
                                                       CancellationToken cancellationToken) =>
        {
            var grain = grainFactory.GetGrain<IEmployeeGrain>(id);
            var result = await grain.GetState();
            return Results.Ok(result);
        })
        .WithTags("Employees")
        .Produces<EmployeeGrainResponse>(StatusCodes.Status200OK);
    }
}
