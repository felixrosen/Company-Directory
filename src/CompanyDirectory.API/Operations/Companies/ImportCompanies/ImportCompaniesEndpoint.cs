using System.Globalization;
using CompanyDirectory.API.Infrastructure.Orleans.Types;
using CompanyDirectory.API.Operations.Companies.Services.Grains.CompanyGrain;
using CompanyDirectory.API.Operations.Companies.Services.Grains.CompanyGrain.Types;
using CompanyDirectory.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CompanyDirectory.API.Operations.Companies.ImportCompanies;

public class ImportCompaniesEndpoint : IEndpoint
{
    public string Route => "/companies";

    public void Bootstrap(WebApplication app, string baseRoute)
    {
        app.MapPost($"{baseRoute}{Route}/import", async (ImportCompaniesFromCsvRowsRequest request,
                                                         [FromServices] IClusterClient cluster) =>
        {
            var companies = ParseImportRequest(request);
            var results = new List<ImportCompaniesFromCsvResult>();
            var tasks = new List<Task>();

            foreach (var chunk in companies.Chunk(50))
            {
                foreach (var c in chunk)
                {
                    if (Guid.TryParse(c.Id, out var id))
                    {
                        var grain = cluster.GetGrain<ICompanyGrain>(id);

                        var t = grain.Handle(new GrainRequest<ImportCompanyRequest>
                        {
                            Request = c,
                        });

                        tasks.Add(t);

                        results.Add(new ImportCompaniesFromCsvResult
                        { Id = c.Id.ToString(), RowResult = CompaniesImportRowResult.Success, });
                    }
                    else
                    {
                        results.Add(new ImportCompaniesFromCsvResult
                        { Id = c.Id.ToString(), RowResult = CompaniesImportRowResult.ValidationError, Message = "Could not parse Id" });
                    }
                }

                await Task.WhenAll(tasks).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
                tasks.Clear();
            }

            return Results.Ok(new ImportCompaniesFromCsvRowsResponse { Results = results, });
        })
        .Produces<ImportCompaniesFromCsvRowsResponse>(StatusCodes.Status200OK)
        .WithTags("Companies");
    }

    public List<ImportCompanyRequest> ParseImportRequest(ImportCompaniesFromCsvRowsRequest request)
    {
        var companies = new List<ImportCompanyRequest>();

        var rows = request.HasHeader
                    ? request.Rows.Skip(1)
                    : request.Rows;

        foreach (var r in rows)
        {
            var columns = r.Split(',', StringSplitOptions.TrimEntries);

            var i = new ImportCompanyRequest
            {
                Id = columns[0],
                CompanyName = columns[1],
                Industry = columns[2],
                Revenue = decimal.Parse(columns[3], CultureInfo.InvariantCulture) / 100, // From cents to dollars
                EmployeeCount = int.Parse(columns[4]),
                FoundedDate = DateTime.ParseExact(columns[5], "M/d/yyyy", CultureInfo.InvariantCulture).ToUtc(),
                CeoName = columns[6],
                HqAddress = columns[7],
                HqCity = columns[8],
                HqState = columns[9] ?? string.Empty,
                HqCountry = columns[10],

                RequestId = Guid.NewGuid(),
            };

            companies.Add(i);
        }

        return companies;
    }
}