using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using vendingmachines.commands.domain.DomainEvents;
using vendingmachines.commands.persistence.Datamodels;

namespace vendingmachines.commands.persistence.Repository;

public class UnitOfWork(ILogger<UnitOfWork> logger, IMongoClient mongoClient, IEventsRepository eventsRepository, 
    IOutboxRepository outboxRepository) : IUnitOfWork
{
    public async Task AtomicSave(EventsDataModel eventDm, BaseDomainEvent evnt)
    {
        using var session = await mongoClient.StartSessionAsync();
        session.StartTransaction();

        try
        {
            await eventsRepository.Save(eventDm, session);
            await outboxRepository.SaveToOutbox(new OutboxDataModel
            {
                BaseDomainEvent = evnt
            }, session);

            await session.CommitTransactionAsync();
            logger.LogInformation("Events and Outbox Transaction Committed successfully");
        }
        catch (Exception ex)
        {
            await session.AbortTransactionAsync();
            logger.LogError("Events and Outbox Transaction failed: {}", ex.Message);
            throw;
        }
    }
}
