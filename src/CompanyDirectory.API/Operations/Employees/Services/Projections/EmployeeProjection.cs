using CompanyDirectory.API.Infrastructure.Mongo;
using CompanyDirectory.API.Services.Projections;

namespace CompanyDirectory.API.Operations.Employees.Services.Projections;

public class EmployeeProjection : IMongoProjection
{
    public static string CollectionName => "projection-employees";

    public required string Id { get; init; }
    public required EmployeeNameProjection Name { get; init; }
    public required string Department { get; init; }
    public required decimal Salary { get; init; }

    public required EmployeeCompanyProjection? Company { get; init; }
    public ProjectionStateType State { get; set; }
}

public class EmployeeNameProjection
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
}

public class EmployeeCompanyProjection
{
    public required string Id { get; init; }
    public required string CompanyName { get; init; }
    public required string Industry { get; set; }
}
