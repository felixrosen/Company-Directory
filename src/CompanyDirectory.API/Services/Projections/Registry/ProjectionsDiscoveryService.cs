using System.Diagnostics;
using System.Reflection;

namespace CompanyDirectory.API.Services.Projections.Registry;

public class ProjectionsDiscoveryService
{
    private List<(Type Type, string Name, string FullName)>? _projectionsDiscovered = null;

    public List<(Type Type, string Name, string FullName)> GetRegisteredProjections()
    {
        if (_projectionsDiscovered is { })
            return _projectionsDiscovered;

        var type = typeof(IProjectionBuilderGrain);

        var types = Assembly.GetAssembly(typeof(IProjectionBuilderGrain))!.GetTypes();
        var typesFromType = types
                            .Select(s => s)
                            .Where(q => type.IsAssignableFrom(q) && !q.IsInterface)
                            .ToList();

        var projectionsDiscovered = new List<(Type, string, string)>();

        foreach (var t in typesFromType)
        {
            if (t is null)
                continue;

            var typeInfo = t.GetTypeInfo();
            var discoveryAttribute = t.GetCustomAttribute<ProjectionsRegistryDiscoveryAttribute>();

            if (discoveryAttribute is null)
                continue;

            if (t.FullName is null)
                continue;

            var targetStateType = discoveryAttribute.Type;
            var grainType = discoveryAttribute.GrainType;

            Debug.WriteLine($"{grainType} - {targetStateType.FullName}");

            projectionsDiscovered.Add(new(targetStateType, grainType, t.FullName));
        }

        _projectionsDiscovered = projectionsDiscovered;
        return projectionsDiscovered;
    }
}