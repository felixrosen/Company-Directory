using CompanyDirectory.API.Infrastructure.Orleans.Types;

namespace CompanyDirectory.API.Operations.Employees.Services.Grains.EmployeeGrain.Types;

[GenerateSerializer]
[Alias("EmployeeGrain.Types.ImportEmployeeRequest")]
public class ImportEmployeeRequest : IGrainRequest
{
    [Id(0)]
    public Guid RequestId { get; init; }

    [Id(1)]
    public required string Id { get; init; }
    [Id(2)]
    public required string FirstName { get; init; }
    [Id(3)]
    public required string LastName { get; init; }
    [Id(4)]
    public required string Department { get; init; }
    [Id(5)]
    public required decimal Salary { get; init; }
    [Id(6)]
    public string? CompanyId { get; init; }
}

[GenerateSerializer]
[Alias("EmployeeGrain.Types.EmployeeGrainResponse")]
public class EmployeeGrainResponse
{
    [Id(0)]
    public required EmployeeGrainState? State { get; set; }
}

[GenerateSerializer]
[Alias("EmployeeGrain.Types.EmployeeCreated")]
public class EmployeeCreated : IEvent
{
    [Id(0)]
    public required string Id { get; init; }
    [Id(1)]
    public required string FirstName { get; init; }
    [Id(2)]
    public required string LastName { get; init; }
    [Id(3)]
    public required string Department { get; init; }
    [Id(4)]
    public required decimal Salary { get; init; }
    [Id(5)]
    public string? CompanyId { get; init; }
    [Id(6)]
    public DateTime EventTimestamp { get; init; }
    [Id(7)]
    public DateTime ActualTimestamp { get; init; }

    public static EmployeeCreated Construct(ImportEmployeeRequest request)
    {
        return new EmployeeCreated
        {
            Id = request.Id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Department = request.Department,
            Salary = request.Salary,
            CompanyId = request.CompanyId,
            EventTimestamp = DateTime.UtcNow,
            ActualTimestamp = DateTime.UtcNow,
        };
    }
}

[GenerateSerializer]
[Alias("EmployeeGrain.Types.EmployeeNameUpdate")]
public class EmployeeNameUpdated : IEvent
{
    [Id(0)]
    public required string FirstName { get; init; }
    [Id(1)]
    public required string LastName { get; init; }
    [Id(2)]
    public DateTime EventTimestamp { get; init; }
    [Id(3)]
    public DateTime ActualTimestamp { get; init; }

    public static EmployeeNameUpdated Construct(ImportEmployeeRequest request)
    {
        return new EmployeeNameUpdated
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            ActualTimestamp = DateTime.UtcNow,
            EventTimestamp = DateTime.UtcNow,
        };
    }
}

[GenerateSerializer]
[Alias("EmployeeGrain.Types.EmployeeDepartmentUpdate")]
public class EmployeeDepartmentUpdated : IEvent
{
    [Id(0)]
    public required string Department { get; init; }
    [Id(1)]
    public DateTime EventTimestamp { get; init; }
    [Id(2)]
    public DateTime ActualTimestamp { get; init; }

    public static EmployeeDepartmentUpdated Construct(ImportEmployeeRequest request)
    {
        return new EmployeeDepartmentUpdated
        {
            Department = request.Department,
            ActualTimestamp = DateTime.UtcNow,
            EventTimestamp = DateTime.UtcNow,
        };
    }
}

[GenerateSerializer]
[Alias("EmployeeGrain.Types.EmployeeSalaryUpdate")]
public class EmployeeSalaryUpdated : IEvent
{
    [Id(0)]
    public required decimal Salary { get; init; }
    [Id(1)]
    public DateTime EventTimestamp { get; init; }
    [Id(2)]
    public DateTime ActualTimestamp { get; init; }


    public static EmployeeSalaryUpdated Construct(ImportEmployeeRequest request)
    {
        return new EmployeeSalaryUpdated
        {
            Salary = request.Salary,
            ActualTimestamp = DateTime.UtcNow,
            EventTimestamp = DateTime.UtcNow,
        };
    }
}