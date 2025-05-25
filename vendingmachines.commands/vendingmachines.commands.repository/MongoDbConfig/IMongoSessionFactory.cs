using MongoDB.Driver;

namespace vendingmachines.commands.persistence.MongoDbConfig;

public interface IMongoSessionFactory
{
    Task<IClientSessionHandle> StartSessionAsync();
}
