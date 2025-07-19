using CompanyDirectory.API.Infrastructure.Mongo;
using CompanyDirectory.API.Operations.Employees.Services.Projections;
using CompanyDirectory.API.Services;
using CompanyDirectory.API.Types;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace CompanyDirectory.API.Operations.Employees.QueryEmployees;

public class EmployeesQueryRequest : PagedRequest
{
    public string? CompanyId { get; init; }    
}

public class EmployeesQueryResponse
{
    public int Count => Employees.Count;
    public List<EmployeeProjection> Employees { get; set; } = [];
}

public class QueryEmployees : IEndpoint
{
    public string Route => "/employees";

    public void Bootstrap(WebApplication app, string baseRoute)
    {
        app.MapPost($"{baseRoute}{Route}/query", async (EmployeesQueryRequest request,
                                                        [FromServices] IMongoClient mongoClient,
                                                        CancellationToken cancellationToken) =>
        {            
            var collection = mongoClient.GetCollection<EmployeeProjection>();
            var query = collection.AsQueryable();

            if (request.CompanyId is not null)
                query = query.Where(q => q.Company != null && q.Company!.Id == request.CompanyId.ToString());

            var employees = await query.OrderBy(o => o.Id)
                                       .Skip(request.Page * request.PageSize)
                                       .Take(request.PageSize)
                                       .ToListAsync(cancellationToken: cancellationToken);

            return Results.Ok(new EmployeesQueryResponse
            {
                Employees = employees
            });
        })
        .Produces<EmployeesQueryResponse>(StatusCodes.Status200OK)
        .WithTags("Employees");
    }
}
