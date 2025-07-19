using CompanyDirectory.API.Operations.Companies.Services.Projections;
using CompanyDirectory.API.Types;

namespace CompanyDirectory.API.Operations.Companies.QueryCompanies;

public class CompaniesQueryRequest : PagedRequest
{
    
}

public class CompaniesQueryResponse
{
    public int Count => Companies.Count;
    public required List<CompanyProjection> Companies { get; init; }
}

public class CompaniesMetricQueryRequest : CompaniesQueryRequest
{
    public EmployeeCountRange? EmployeeCountRange { get; init; }
    public RevenueRange? RevenueRange { get; init; }
}

public class CompaniesMetricsQueryResponse
{
    public required List<CompanyEmployeeRange> EmployeeRanges { get; init; }
    public required List<CompanyRevenueRange> RevenueRanges { get; init; }
    public required List<CompanyProjection> Companies { get; init; }
}

public class EmployeeCountRange
{
    public int Min { get; init; }
    public int Max { get; init; }
}

public class RevenueRange
{
    public decimal Min { get; init; }
    public decimal Max { get; init; }
}

public class CompanyEmployeeRange
{
    public required int Min { get; init; }
    public required int Max { get; init; }
    public required int Count { get; init; }
}

public class CompanyRevenueRange
{
    public required decimal Min { get; init; }
    public required decimal Max { get; init; }
    public required int Count { get; init; }
}