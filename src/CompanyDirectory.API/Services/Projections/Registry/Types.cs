using CompanyDirectory.API.Infrastructure.Orleans.Types;

namespace CompanyDirectory.API.Services.Projections.Registry;

[AttributeUsage(AttributeTargets.Class)]
public class ProjectionsRegistryDiscoveryAttribute : Attribute
{
    public Type Type { get; private set; }
    public string GrainType { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetStateType">The target state type</param>
    /// <param name="grainType">Must be the same a the [GrainType] attribute</param>
    public ProjectionsRegistryDiscoveryAttribute(Type targetStateType, string grainType)
    {
        Type = targetStateType;
        GrainType = grainType;
    }
}

[Alias("Projections.Registry.IProjectionBuilder")]
public interface IProjectionBuilderGrain : IGrainWithGuidCompoundKey
{
    [Alias("Construct")]
    Task<ConstructProjectionResult> Construct(ConstructProjectionRequest request);
}

[GenerateSerializer]
[Alias("Projections.Registry.ConstructProjectionRequest")]
public class ConstructProjectionRequest
{
    [Id(0)]
    public required Guid Id { get; init; }

    [Id(1)]
    public required object State { get; init; }

    [Id(2)]
    public required List<IEvent> Events { get; init; } = [];

    [Id(3)]
    public required Type StateType { get; init; }

    [Id(4)]
    public required List<(string Name, string FullName)> Projections { get; init; }
}

[GenerateSerializer]
[Alias("Projections.Registry.ConstructProjectionResult")]
public class ConstructProjectionResult
{
    [Id(0)]
    public ConstructProjectionResultStatus Status { get; init; }
    [Id(1)]
    public Dictionary<string, string>? ValidationErrors { get; set; }

    public static ConstructProjectionResult ConstructSuccess() => new() { Status = ConstructProjectionResultStatus.Success, ValidationErrors = null };
}

[GenerateSerializer]
public enum ConstructProjectionResultStatus
{
    Success,
    ValidationError,
    Error,
}
