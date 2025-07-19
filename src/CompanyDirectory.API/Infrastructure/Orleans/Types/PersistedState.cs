namespace CompanyDirectory.API.Infrastructure.Orleans.Types;

public interface IPersistedState
{
    string? Id { get; set; }
}


[GenerateSerializer]
[Alias("PersistedState")]
public class PersistedState : IPersistedState
{
    [Id(0)]
    public string? Id { get; set; }

    [Id(1)]
    public List<IEvent> Events { get; set; } = [];
    [Id(2)]
    public List<IGrainRequest> Requests { get; set; } = [];
}
