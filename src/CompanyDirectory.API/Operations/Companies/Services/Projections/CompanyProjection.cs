using CompanyDirectory.API.Infrastructure.Mongo;
using CompanyDirectory.API.Services.Projections;

namespace CompanyDirectory.API.Operations.Companies.Services.Projections;

public record CompanyProjection : IMongoProjection
{
    public static string CollectionName => "projection-companies";

    public required string Id { get; init; }

    public required string CompanyName { get; init; }
    public required string CeoName { get; init; }

    public required string Industry { get; init; }
    public required DateTime FoundedDate { get; init; }

    public required CompanyProjectionMetric Metrics { get; init; }

    public required CompanyProjectionAddress Address { get; init; }
    public required DateTime UpdatedDateTime { get; init; }

    public required ProjectionStateType State { get; set; }

    public List<CompanyProjectionEmployee> Employees { get; init; } = [];
}

public record CompanyProjectionMetric
{
    private decimal _revenue;
    public required decimal Revenue
    {
        get { return _revenue; }
        set
        {
            _revenue = value;
            RevenueRangeMin = _revenue switch
            {
                < 100_000 => 0,
                < 200_000 => 100_000,
                < 500_000 => 200_000,
                < 1_000_000 => 500_000,
                < 10_000_000 => 1_000_000,
                _ => 1_000_000,
            };

            RevenueRangeMax = _revenue switch
            {
                <= 100_000 => 100_000,
                <= 200_000 => 200_000,
                <= 500_000 => 500_000,
                <= 1_000_000 => 1_000_000,
                <= 10_000_000 => 10_000_000,
                _ => decimal.MaxValue,
            };
        }
    }
    public decimal RevenueRangeMin { get; private set; }
    public decimal RevenueRangeMax { get; private set; }

    private int? _employeeCount;
    public int? EmployeeCount
    {
        get { return _employeeCount; }
        set
        {
            _employeeCount = value;

            EmployeeCountRangeMin = _employeeCount switch
            {
                null => 0,
                < 100 => 0,
                >= 100 and < 200 => 100,
                >= 200 and < 500 => 200,
                >= 500 and < 1_000 => 500,
                _ => 1_000,
            };

            EmployeeCountRangeMax = _employeeCount switch
            {
                null => 0,
                <= 100 => 100,
                > 100 and <= 200 => 200,
                > 200 and <= 500 => 500,
                > 500 and <= 1_000 => 1_000,
                _ => int.MaxValue,
            };
        }
    }
    public int EmployeeCountRangeMin { get; private set; }
    public int EmployeeCountRangeMax { get; private set; }
}

public record CompanyProjectionAddress
{
    public required string HqAddress { get; init; }

    public required string HqCity { get; init; }

    public required string HqState { get; init; }

    public required string HqCountry { get; init; }
}

public record CompanyProjectionEmployee
{
    public required string Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required decimal Salary { get; set; }
    public required string Department { get; set; }
}
