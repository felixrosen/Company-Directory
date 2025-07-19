using CompanyDirectory.API.Infrastructure.Orleans.Types;
using CompanyDirectory.API.Services;

namespace CompanyDirectory.API.Operations.Companies.Services.Grains.CompanyGrain.Types;

[GenerateSerializer]
[Alias("CreateCompanyRequest")]
public class ImportCompanyRequest : IGrainRequest
{
    [Id(0)]
    public required Guid RequestId { get; init; }

    [Id(1)]
    public required string Id { get; init; }
    [Id(2)]
    public required string CompanyName { get; init; }
    [Id(3)]
    public required string Industry { get; init; }
    [Id(4)]
    public required decimal Revenue { get; init; }
    [Id(5)]
    public required int EmployeeCount { get; init; }
    [Id(6)]
    public required DateTime FoundedDate { get; init; }
    [Id(7)]
    public required string CeoName { get; init; }
    [Id(8)]
    public required string HqAddress { get; init; }
    [Id(9)]
    public required string HqCity { get; init; }
    [Id(10)]
    public required string HqState { get; init; }
    [Id(11)]
    public required string HqCountry { get; init; }
}

[GenerateSerializer]
[Alias("DeleteCompanyRequest")]
public class DeleteCompanyRequest : IGrainRequest
{
    [Id(0)]
    public required Guid RequestId { get; init; }

    [Id(1)]
    public required string Id { get; init; }
}

[GenerateSerializer]
[Alias("CompanyCreated")]
public class CompanyCreated : IEvent
{
    [Id(0)]
    public required string Id { get; init; }
    [Id(1)]
    public required string CompanyName { get; init; }
    [Id(2)]
    public required string Industry { get; init; }
    [Id(3)]
    public required decimal Revenue { get; init; }
    [Id(4)]
    public required int EmployeeCount { get; init; }
    [Id(5)]
    public required DateTime FoundedDate { get; init; }
    [Id(6)]
    public required string CeoName { get; init; }
    [Id(7)]
    public required string HqAddress { get; init; }
    [Id(8)]
    public required string HqCity { get; init; }
    [Id(9)]
    public required string HqState { get; init; }
    [Id(10)]
    public required string HqCountry { get; init; }

    [Id(11)]
    public required DateTime EventTimestamp { get; init; }
    [Id(12)]
    public required DateTime ActualTimestamp { get; init; }

    public static CompanyCreated Construct(ImportCompanyRequest request)
    {
        return new CompanyCreated
        {
            Id = request.Id,
            CompanyName = request.CompanyName,
            Industry = request.Industry,
            Revenue = request.Revenue,
            EmployeeCount = request.EmployeeCount,
            FoundedDate = request.FoundedDate.ToUtc(),
            CeoName = request.CeoName,
            HqAddress = request.HqAddress,
            HqCity = request.HqCity,
            HqState = request.HqState,
            HqCountry = request.HqCountry,

            EventTimestamp = DateTime.UtcNow,
            ActualTimestamp = DateTime.UtcNow,
        };
    }
}

[GenerateSerializer]
[Alias("CompanyAddressUpdated")]
public class CompanyAddressUpdated : IEvent
{
    [Id(0)]
    public required string HqAddress { get; init; }
    [Id(1)]
    public required string HqCity { get; init; }
    [Id(2)]
    public required string HqState { get; init; }
    [Id(3)]
    public required string HqCountry { get; init; }
    [Id(4)]
    public DateTime EventTimestamp { get; init; }
    [Id(5)]
    public DateTime ActualTimestamp { get; init; }

    public static CompanyAddressUpdated Construct(ImportCompanyRequest request)
    {
        return new CompanyAddressUpdated
        {
            HqAddress = request.HqAddress,
            HqCity = request.HqCity,
            HqState = request.HqState,
            HqCountry = request.HqCountry,

            ActualTimestamp = DateTime.UtcNow,
            EventTimestamp = DateTime.UtcNow,
        };
    }
}

[GenerateSerializer]
[Alias("CompanyInfoUpdate")]
public class CompanyInfoUpdated : IEvent
{
    [Id(0)]
    public required string CompanyName { get; set; }
    [Id(1)]
    public required string CeoName { get; set; }
    [Id(2)]
    public required string Industry { get; set; }
    [Id(3)]
    public required DateTime FoundedDate { get; set; }

    [Id(4)]
    public DateTime EventTimestamp { get; init; }
    [Id(5)]
    public DateTime ActualTimestamp { get; init; }

    public static CompanyInfoUpdated Construct(ImportCompanyRequest request)
    {
        return new CompanyInfoUpdated
        {
            CeoName = request.CeoName,
            CompanyName = request.CompanyName,
            FoundedDate = request.FoundedDate.ToUtc(),
            Industry = request.Industry,

            ActualTimestamp = DateTime.UtcNow,
            EventTimestamp = DateTime.UtcNow,
        };
    }
}

[GenerateSerializer]
[Alias("CompanyMetricsUpdated")]
public class CompanyMetricsUpdated : IEvent
{
    [Id(0)]
    public required decimal Revenue { get; set; }

    [Id(1)]
    public required int EmployeeCount { get; set; }

    [Id(3)]
    public DateTime EventTimestamp { get; init; }
    [Id(4)]
    public DateTime ActualTimestamp { get; init; }

    public static CompanyMetricsUpdated Construct(ImportCompanyRequest request)
    {
        return new CompanyMetricsUpdated
        {
            EmployeeCount = request.EmployeeCount,
            Revenue = request.Revenue,

            ActualTimestamp = DateTime.UtcNow,
            EventTimestamp = DateTime.UtcNow,
        };
    }
}

[GenerateSerializer]
[Alias("CompanyDeleted")]
public class CompanyDeleted : IEvent
{
    [Id(0)]
    public required DateTime EventTimestamp { get; init; }

    [Id(1)]
    public required DateTime ActualTimestamp { get; init; }

    public static CompanyDeleted Construct()
    {
        return new CompanyDeleted
        {
            ActualTimestamp = DateTime.UtcNow,
            EventTimestamp = DateTime.UtcNow,
        };
    }
}