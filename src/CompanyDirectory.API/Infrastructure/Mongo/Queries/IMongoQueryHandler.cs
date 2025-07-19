namespace CompanyDirectory.API.Infrastructure.Mongo.Queries
{
    public interface IMongoQueryHandler<TQuery, TResult>
    {
        Task<TResult> Handle(TQuery query);
    }
}