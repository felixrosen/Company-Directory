namespace CompanyDirectory.API.Infrastructure.Orleans.Types;

public interface IGrainRequest
{
    Guid RequestId { get; init; }
}

[GenerateSerializer]
[Alias("GrainRequest")]
public class GrainRequest<T> where T : IGrainRequest
{
    [Id(0)]
    public required T Request { get; init; }
}
