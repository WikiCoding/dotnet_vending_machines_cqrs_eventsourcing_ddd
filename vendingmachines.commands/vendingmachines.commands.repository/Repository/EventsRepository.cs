using MongoDB.Driver;
using vendingmachines.commands.persistence.Datamodels;
using vendingmachines.commands.persistence.MongoDbConfig;

namespace vendingmachines.commands.persistence.Repository;

public class EventsRepository : IEventsRepository
{
    private const string CollectionName = "machines";
    private readonly MongoConfig _mongoConfig;
    private readonly IMongoDatabase _database;

    public EventsRepository(MongoConfig mongoConfig)
    {
        _mongoConfig = mongoConfig;
        _database = _mongoConfig.GetDatabaseConnection();
    }

    public async Task<List<EventsDataModel>> FindByAggId(string aggId)
    {
        return await _database.GetCollection<EventsDataModel>(CollectionName).Find(el => el.AggregateId == aggId).SortBy(x => x.Version).ToListAsync();
    }

    public async Task<EventsDataModel> Save(EventsDataModel eventDm)
    {
        await _database.GetCollection<EventsDataModel>(CollectionName).InsertOneAsync(eventDm);

        return eventDm;
    }
}
