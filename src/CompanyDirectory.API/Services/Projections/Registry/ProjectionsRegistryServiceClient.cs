using Orleans.Runtime.Services;
using Orleans.Services;

namespace CompanyDirectory.API.Services.Projections.Registry;

[GenerateSerializer]
[Alias("Projections.Registry.GetRegisteredProjectionsFromTypeRequest")]
public class GetRegisteredProjectionsFromTypeRequest
{
    [Id(0)]
    public required Type Type { get; set; }
}

[GenerateSerializer]
[Alias("Projections.Registry.GetRegisteredProjectionsFromNameRequest")]
public class GetRegisteredProjectionsFromNameRequest
{
    [Id(0)]
    public required string Name { get; set; }
}

[GenerateSerializer]
[Alias("Projections.Registry.GetRegisteredProjectionsResponse")]
public class GetRegisteredProjectionsResponse
{
    [Id(0)]
    public required string? FromTypeName { get; set; }
    [Id(1)]
    public required Type? Type { get; set; }
    [Id(2)]
    public required List<(string Name, string FullName)> Projections { get; set; }
}

[Alias("Projections.Registry.IProjectionsRegistryServiceClient")]
public interface IProjectionsRegistryServiceClient : IGrainServiceClient<IProjectionsRegistryService>, IProjectionsRegistryService
{

}

public class ProjectionsRegistryServiceClient : GrainServiceClient<IProjectionsRegistryService>, IProjectionsRegistryServiceClient
{
    public ProjectionsRegistryServiceClient(IServiceProvider serviceProvider) : base(serviceProvider)
    { }

    private IProjectionsRegistryService GrainService => GetGrainService(CurrentGrainReference.GrainId);

    public Task<GetRegisteredProjectionsResponse> GetProjections(GetRegisteredProjectionsFromNameRequest request)
    {
        return GrainService.GetProjections(request);
    }

    public Task<GetRegisteredProjectionsResponse> GetProjections(GetRegisteredProjectionsFromTypeRequest request)
    {
        return GrainService.GetProjections(request);
    }
}
