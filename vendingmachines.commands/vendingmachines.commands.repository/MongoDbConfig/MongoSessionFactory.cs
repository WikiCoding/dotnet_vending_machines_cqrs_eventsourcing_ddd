using MongoDB.Driver;

namespace vendingmachines.commands.persistence.MongoDbConfig;

public class MongoSessionFactory : IMongoSessionFactory
{
    private readonly IMongoClient _mongoClient;

    public MongoSessionFactory(IMongoClient mongoClient)
    {
        _mongoClient = mongoClient;
    }

    public Task<IClientSessionHandle> StartSessionAsync()
    {
        return _mongoClient.StartSessionAsync();
    }
}
