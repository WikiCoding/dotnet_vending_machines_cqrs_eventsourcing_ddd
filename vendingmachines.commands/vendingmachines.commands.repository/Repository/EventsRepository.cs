using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using vendingmachines.commands.persistence.Datamodels;

namespace vendingmachines.commands.persistence.Repository;

public class EventsRepository : IEventsRepository
{
    private const string DatabaseName = "vending_machines";
    private const string CollectionName = "machines";
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<EventsDataModel> _mongoCollection;
    private readonly ILogger<EventsRepository> _logger;

    public EventsRepository(IMongoClient mongoClient, ILogger<EventsRepository> logger)
    {
        _database = mongoClient.GetDatabase(DatabaseName);
        _mongoCollection = _database.GetCollection<EventsDataModel>(CollectionName);
        _logger = logger;
    }

    public async Task<List<EventsDataModel>> FindByAggId(string aggId)
    {
        return await _mongoCollection.Find(el => el.AggregateId == aggId).SortBy(x => x.Version).ToListAsync();
    }

    public async Task<IEnumerable<EventsDataModel>> FindAll()
    {
        return await _mongoCollection.Find(_ => true).ToListAsync();
    }

    public async Task<EventsDataModel> Save(EventsDataModel eventDm, IClientSessionHandle session)
    {
        await _mongoCollection.InsertOneAsync(session, eventDm);

        _logger.LogInformation("Event with Id {} added to Transaction", eventDm.AggregateId);
        return eventDm;
    }
}
