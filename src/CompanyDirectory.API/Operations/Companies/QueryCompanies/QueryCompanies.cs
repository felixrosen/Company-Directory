using CompanyDirectory.API.Infrastructure.Mongo;
using CompanyDirectory.API.Operations.Companies.Services.Projections;
using CompanyDirectory.API.Services;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace CompanyDirectory.API.Operations.Companies.QueryCompanies;

public class QueryCompanies : IEndpoint
{
    public string Route => "/companies";

    public void Bootstrap(WebApplication app, string baseRoute)
    {
        app.MapPost($"{baseRoute}{Route}/query", async (CompaniesQueryRequest request,
                                                        IMongoClient mongoClient) =>
        {
            var collection = mongoClient.GetCollection<CompanyProjection>();

            var query = collection.AsQueryable();


            var result = await query.OrderBy(o => o.Id).Skip(request.Page * request.PageSize).Take(request.PageSize).ToListAsync();

            return Results.Ok(new CompaniesQueryResponse
            {
                Companies = result,
            });
        })
        .Produces<CompaniesQueryResponse>(StatusCodes.Status200OK)
        .WithTags("Companies")
        .WithDescription("Query companies");

        app.MapPost($"{baseRoute}{Route}/query-metrics", async (CompaniesMetricQueryRequest request,
                                                                IMongoClient mongoClient) =>
        {
            if (request.EmployeeCountRange is null && request.RevenueRange is null)
                return Results.BadRequest("At least one of EmployeeCountRange or RevenueRange must be specified");

            var collection = mongoClient.GetCollection<CompanyProjection>();

            var query = collection.AsQueryable();

            if (request.EmployeeCountRange is not null)
            {
                query = query.Where(c => c.Metrics.EmployeeCountRangeMin >= request.EmployeeCountRange.Min
                                         && c.Metrics.EmployeeCountRangeMax <= request.EmployeeCountRange.Max);
            }

            if (request.RevenueRange is not null)
            {
                query = query.Where(c => c.Metrics.RevenueRangeMin >= request.RevenueRange.Min
                                         && c.Metrics.RevenueRangeMax <= request.RevenueRange.Max);
            }

            var result = await query.OrderBy(o => o.Id).Skip(request.Page * request.PageSize).Take(request.PageSize).ToListAsync();

            var employRangeAggregated = (from r in result
                                         group r by new { r.Metrics.EmployeeCountRangeMin, r.Metrics.EmployeeCountRangeMax } into g
                                         select new CompanyEmployeeRange
                                         {
                                             Min = g.Key.EmployeeCountRangeMin,
                                             Max = g.Key.EmployeeCountRangeMax,
                                             Count = g.Count()
                                         }).OrderBy(o => o.Min).ToList();

            var revenueRangeAggregated = (from r in result
                                          group r by new { r.Metrics.RevenueRangeMin, r.Metrics.RevenueRangeMax } into g
                                          select new CompanyRevenueRange
                                          {
                                              Min = g.Key.RevenueRangeMin,
                                              Max = g.Key.RevenueRangeMax,
                                              Count = g.Count()
                                          }).OrderBy(o => o.Min).ToList();

            return Results.Ok(new CompaniesMetricsQueryResponse
            {
                Companies = result,
                RevenueRanges = revenueRangeAggregated,
                EmployeeRanges = employRangeAggregated
            });
        })
        .Produces<CompaniesMetricsQueryResponse>(StatusCodes.Status200OK)
        .WithTags("Companies");
    }
}