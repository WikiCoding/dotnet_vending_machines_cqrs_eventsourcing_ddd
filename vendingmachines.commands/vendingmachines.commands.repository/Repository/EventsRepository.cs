using MongoDB.Driver;
using vendingmachines.commands.persistence.Datamodels;

namespace vendingmachines.commands.persistence.Repository;

public class EventsRepository : IEventsRepository
{
    private const string CollectionName = "machines";
    private readonly IMongoClient _mongoClient;
    private readonly IMongoDatabase _database;

    public EventsRepository(IMongoClient mongoClient)
    {
        _mongoClient = mongoClient;
        _database = _mongoClient.GetDatabase(CollectionName);
    }

    public async Task<List<EventsDataModel>> FindByAggId(string aggId)
    {
        return await _database.GetCollection<EventsDataModel>(CollectionName).Find(el => el.AggregateId == aggId).SortBy(x => x.Version).ToListAsync();
    }

    public async Task<IEnumerable<EventsDataModel>> FindAll()
    {
        return await _database.GetCollection<EventsDataModel>(CollectionName).Find(_ => true).ToListAsync();
    }

    public async Task<EventsDataModel> Save(EventsDataModel eventDm, IClientSessionHandle session)
    {
        await _database.GetCollection<EventsDataModel>(CollectionName).InsertOneAsync(eventDm);

        return eventDm;
    }
}
