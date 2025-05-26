using MongoDB.Driver;
using vendingmachines.commands.persistence.Datamodels;

namespace vendingmachines.commands.persistence.Repository;

public interface IOutboxRepository
{
    Task<List<OutboxDataModel>> FindAllNotProcessed();
    Task SaveToOutbox(OutboxDataModel outboxDataModel, IClientSessionHandle session);
    Task UpdateOutboxEntry(OutboxDataModel outboxDataModel);
}
