using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using vendingmachines.commands.persistence.Datamodels;

namespace vendingmachines.commands.persistence.Repository;

public class OutboxRepository : IOutboxRepository
{
    private const string DatabaseName = "vending_machines";
    private const string CollectionName = "outbox";
    private readonly ILogger<OutboxRepository> _logger;
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<OutboxDataModel> _mongoCollection;

    public OutboxRepository(ILogger<OutboxRepository> logger, IMongoClient mongoClient)
    {
        _database = mongoClient.GetDatabase(DatabaseName);
        _mongoCollection = _database.GetCollection<OutboxDataModel>(CollectionName);
        _logger = logger;
    }

    public async Task<List<OutboxDataModel>> FindAllNotProcessed()
    {
        return await _mongoCollection.Find(odm => !odm.IsProcessed).ToListAsync();
    }

    public async Task SaveToOutbox(OutboxDataModel outboxDataModel, IClientSessionHandle session)
    {
        await _mongoCollection.InsertOneAsync(session, outboxDataModel);
        _logger.LogInformation("Outbox with Id {} added to transaction", outboxDataModel.OutboxId);
    }

    public async Task UpdateOutboxEntry(OutboxDataModel outboxDataModel)
    {
        outboxDataModel.IsProcessed = true;

        var filter = Builders<OutboxDataModel>.Filter.Eq(x => x.OutboxId, outboxDataModel.OutboxId);
        var update = Builders<OutboxDataModel>.Update.Set(x => x.IsProcessed, true);

        await _mongoCollection.UpdateOneAsync(filter, update);

        _logger.LogInformation("Outbox entry with Id {OutboxId} marked as processed", outboxDataModel.OutboxId);
    }
}
