using System.Globalization;
using CompanyDirectory.API.Infrastructure.Orleans.Types;
using CompanyDirectory.API.Operations.Employees.Services.Grains.EmployeeGrain;
using CompanyDirectory.API.Operations.Employees.Services.Grains.EmployeeGrain.Types;
using CompanyDirectory.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CompanyDirectory.API.Operations.Employees.ImportEmployees;

public class ImportEmployeesEndpoint : IEndpoint
{
    public string Route => "/employees";

    public void Bootstrap(WebApplication app, string baseRoute)
    {
        app.MapPost($"{baseRoute}{Route}/import", async (ImportEmployeesFromCsvRowsRequest request,
                                                         [FromServices] IClusterClient cluster) =>
        {
            var importEmployeeRequests = ParseImportRequest(request);
            var results = new List<ImportEmployeesFromCsvResult>();
            var tasks = new List<Task>();

            foreach (var chunk in importEmployeeRequests.Chunk(100))
            {
                foreach (var c in chunk)
                {
                    if (Guid.TryParse(c.Id, out var id))
                    {
                        var grain = cluster.GetGrain<IEmployeeGrain>(id);
                        var t = grain.Handle(new GrainRequest<ImportEmployeeRequest>
                        {
                            Request = c,
                        });

                        tasks.Add(t);

                        results.Add(new ImportEmployeesFromCsvResult
                        {
                            Id = c.Id.ToString(),
                            RowResult = EmployeesImportRowResult.Success,
                        });
                    }
                    else
                    {
                        results.Add(new ImportEmployeesFromCsvResult { Id = c.Id.ToString(), RowResult = EmployeesImportRowResult.ValidationError, Message = "Could not parse Id", });
                    }
                }

                await Task.WhenAll(tasks).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
                tasks.Clear();
            }

            return Results.Ok(new ImportEmployeesFromCsvRowsResponse { Results = results, });
        })
        .Produces<ImportEmployeesFromCsvResult>(StatusCodes.Status200OK)
        .WithTags("Employees");
    }

    private static List<ImportEmployeeRequest> ParseImportRequest(ImportEmployeesFromCsvRowsRequest request)
    {
        var rows = request.HasHeader ? request.Rows.Skip(1) : request.Rows;

        return [.. rows.Select(row =>
        {
            var columns = row.Split(',', StringSplitOptions.TrimEntries);

            return new ImportEmployeeRequest
            {
                Id = columns[0],
                FirstName = columns[1],
                LastName = columns[2],
                Department = columns[3],
                Salary = decimal.Parse(columns[4].Replace("$", string.Empty), CultureInfo.InvariantCulture),
                CompanyId = columns[5],
            };
        })];
    }
}