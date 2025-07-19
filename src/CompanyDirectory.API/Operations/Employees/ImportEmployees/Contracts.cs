using System.ComponentModel;

namespace CompanyDirectory.API.Operations.Employees.ImportEmployees;

public class ImportEmployeesFromCsvRowsRequest
{
    public required List<string> Rows { get; init; }

    [DefaultValue(true)]
    public required bool HasHeader { get; init; }
}

public class ImportEmployeesFromCsvRowsResponse
{
    public required List<ImportEmployeesFromCsvResult> Results { get; init; }
}

public class ImportEmployeesFromCsvResult
{
    public required string Id { get; init; }
    public string? Message { get; init; }
    public required EmployeesImportRowResult RowResult { get; init; }
}

public enum EmployeesImportRowResult
{
    Success,
    ValidationError,
}
