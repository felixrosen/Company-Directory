namespace CompanyDirectory.API.Services
{
    public interface IEndpoint
    {
        void Bootstrap(WebApplication app, string baseRoute);
        string Route { get; }
    }

}
