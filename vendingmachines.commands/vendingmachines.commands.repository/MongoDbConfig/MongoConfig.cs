using MongoDB.Driver;

namespace vendingmachines.commands.persistence.MongoDbConfig;

public class MongoConfig
{
    private const string DatabaseName = "vending_machines";
    private IMongoDatabase _database;
    public MongoConfig()
    {
        Console.WriteLine("Initializing mongo config");
        var connectionUri = "mongodb://mongouser:mongopass@localhost:27017/machines?authSource=admin";

        var client = new MongoClient(connectionUri);

        _database = client.GetDatabase(DatabaseName);
    }

    public IMongoDatabase GetDatabaseConnection()
    {
        return _database;
    }
}
