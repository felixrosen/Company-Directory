using CompanyDirectory.API.Infrastructure.Mongo.Queries;
using CompanyDirectory.API.Infrastructure.Orleans;
using CompanyDirectory.API.Operations.Companies.Services.Projections;
using CompanyDirectory.API.Operations.Employees.Services.Projections;
using CompanyDirectory.API.Services;
using CompanyDirectory.API.Services.Projections;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                              .AddJsonFile("appsettings.json")
                                              .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true)
                                              .Build();

builder.Services.AddSerilog(options =>
{
    options.WriteTo.Console();
    options.Enrich.FromLogContext();
    options.ReadFrom.Configuration(configuration);
});

builder.Services.AddOpenApi(options => options.AddDocumentTransformer((document, _, _) =>
{
    document.Info = new OpenApiInfo
    {
        Title = "Company Directory API Reference",
        Version = "v1",        
    };

    return Task.CompletedTask;
}));

builder.Services.AddHttpClient();

builder.Services.AddSingleton<IProjectionBuilderQueue, ProjectionBuilderQueue>();
builder.Services.AddHostedService<ProjectionBuilderQueueService>();

builder.Services.AddSingleton<ICompanyProjectionStore, CompanyProjectionStore>();
builder.Services.AddSingleton<IEmployeeProjectionStore, EmployeeProjectionStore>();

builder.Services.AddTransient<IMongoQueryHandler<GetGrainsIdQueryHandler.Query, GetGrainsIdQueryHandler.Result>, GetGrainsIdQueryHandler>();

builder.SetupOrleans();

var app = builder.Build();

BootstrapEndpoints(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(o =>
    {
        o.Title = "Company Directory API";
        o.Layout = ScalarLayout.Modern;
        o.OperationSorter = OperationSorter.Method;
        o.ShowSidebar = true;
        //o.oper
    });
}

app.UseHttpsRedirection();

app.Run();

static void BootstrapEndpoints(WebApplication app)
{
    var endpoints = System.Reflection.Assembly.GetExecutingAssembly()
                                              .GetTypes()
                                              .Where(type => typeof(IEndpoint).IsAssignableFrom(type) && !type.IsInterface);
    foreach (var type in endpoints)
    {
        var handler = (IEndpoint)Activator.CreateInstance(type)!;
        handler.Bootstrap(app, "/api");
    }
}
