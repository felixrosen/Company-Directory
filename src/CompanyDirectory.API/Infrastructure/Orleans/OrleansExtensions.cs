using System.Reflection;
using CompanyDirectory.API.Infrastructure.Orleans.Types;
using CompanyDirectory.API.Operations.Companies.Services.Grains.CompanyGrain;
using CompanyDirectory.API.Services.Projections.Registry;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using Orleans.Configuration;
using Orleans.Providers.MongoDB.StorageProviders.Serializers;

namespace CompanyDirectory.API.Infrastructure.Orleans;

public static class OrleansExtensions
{
    public static void SetupOrleans(this WebApplicationBuilder builder)
    {
        builder.Host.UseOrleans(siloBuilder =>
        {
            var mongoConnectionString = builder.Configuration.GetValue<string>("MongoDb:ConnectionString");

            siloBuilder.AddActivityPropagation();

            siloBuilder.AddGrainService<ProjectionsRegistryService>();

            siloBuilder.ConfigureServices(services =>
            {
                services.AddSingleton<PlacementStrategy, PreferLocalPlacement>();
                services.AddSingleton<IProjectionsRegistryServiceClient, ProjectionsRegistryServiceClient>();
            });

            siloBuilder.Configure<GrainCollectionOptions>(options =>
            {
                // Set the value of CollectionAge to 10 minutes for all grain
                options.CollectionAge = TimeSpan.FromMinutes(10);

                // Override the value of CollectionAge to 5 minutes for MyGrainImplementation
                options.ClassSpecificCollectionAge[typeof(CompanyGrain).FullName!] = TimeSpan.FromMinutes(2);
            });

            RegisterSubTypesOfCommandAndEvent();

            var objectSerializer = new ObjectSerializer(ObjectSerializer.AllAllowedTypes);

            var conventionPack = new ConventionPack
            {
                new EnumRepresentationConvention(BsonType.String)
            };

            ConventionRegistry.Register("EnumStringConvention", conventionPack, t => true);

            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
            BsonSerializer.RegisterSerializer(objectSerializer);

            siloBuilder.UseMongoDBClient(mongoConnectionString)
                       .AddMongoDBGrainStorageAsDefault(options =>
                       {
                           options.DatabaseName = "company-directory";
                           options.CollectionPrefix = "grain-state-";

                           options.GrainStateSerializer = new BsonGrainStateSerializer();
                       });

            siloBuilder.UseDashboard(options =>
            {
                //options.HostSelf = false;
                //options.
            });

            siloBuilder.Configure<ClusterOptions>(options =>
            {
                options.ClusterId = "company-directory-cluster";
                options.ServiceId = "company-directory-cluster";
            }).UseLocalhostClustering();
        });
    }

    private static void RegisterSubTypesOfCommandAndEvent()
    {
        var commandType = typeof(IGrainRequest);
        var eventType = typeof(IEvent);

        var types = Assembly.GetAssembly(typeof(CompanyGrain))!.GetTypes();
        var typesFromType = types
                            .Select(s => s)
                            .Where(q => (commandType.IsAssignableFrom(q) || eventType.IsAssignableFrom(q)) && !q.IsInterface)
                            .ToList();

        foreach (var t in typesFromType)
        {
            BsonClassMap.LookupClassMap(t);
        }
    }
}
