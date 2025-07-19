using Orleans.Services;

namespace CompanyDirectory.API.Services.Projections.Registry;

[Alias("Projections.Registry.IProjectionsRegistryService")]
public interface IProjectionsRegistryService : IGrainService
{
    [Alias("GetProjectionsByName")]
    Task<GetRegisteredProjectionsResponse> GetProjections(GetRegisteredProjectionsFromNameRequest request);
    [Alias("GetProjectionsByType")]
    Task<GetRegisteredProjectionsResponse> GetProjections(GetRegisteredProjectionsFromTypeRequest request);
}

public class ProjectionsRegistryService : GrainService, IProjectionsRegistryService
{
    private Dictionary<Type, List<(string, string)>> _registeredProjections = [];

    public ProjectionsRegistryService(GrainId id,
                                      Silo silo,
                                      ILoggerFactory loggerFactory) : base(id, silo, loggerFactory)
    {
    }


    public override Task Init(IServiceProvider serviceProvider) => base.Init(serviceProvider);

    public override Task Start()
    {
        var projections = new ProjectionsDiscoveryService().GetRegisteredProjections();
        _registeredProjections = (from p in projections
                                  group p by p.Type into g
                                  select new
                                  {
                                      Type = g.Key,
                                      GrainTypes = g.Select(p => (p.Name, p.FullName)).ToList(),
                                  })
                                 .ToDictionary(k => k.Type, v => v.GrainTypes);
        return base.Start();
    }

    public override Task Stop()
    {
        return base.Stop();
    }

    public Task<GetRegisteredProjectionsResponse> GetProjections(GetRegisteredProjectionsFromNameRequest request)
    {
        var type = Type.GetType(request.Name);

        if (type is null)
        {
            return Task.FromResult(new GetRegisteredProjectionsResponse { Type = null, FromTypeName = null, Projections = [] });
        }

        return GetProjections(new GetRegisteredProjectionsFromTypeRequest { Type = type });
    }

    public Task<GetRegisteredProjectionsResponse> GetProjections(GetRegisteredProjectionsFromTypeRequest request)
    {
        var projections = _registeredProjections.TryGetValue(request.Type, out var value) ? value : [];

        return Task.FromResult(new GetRegisteredProjectionsResponse
        {
            FromTypeName = request.Type.Name,
            Type = request.Type,
            Projections = projections
        });
    }
}
