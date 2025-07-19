using System.ComponentModel;

namespace CompanyDirectory.API.Operations.Companies.ImportCompanies;

public class ImportCompaniesFromCsvRowsRequest
{
    public required List<string> Rows { get; init; }

    [DefaultValue(true)]
    public required bool HasHeader { get; init; }
}

public class ImportCompaniesFromCsvRowsResponse
{
    public required List<ImportCompaniesFromCsvResult> Results { get; init; }
}

public class ImportCompaniesFromCsvResult
{
    public required string Id { get; init; }
    public string? Message { get; init; }
    public required CompaniesImportRowResult RowResult { get; init; }
}

public enum CompaniesImportRowResult
{
    Success,
    ValidationError,
}