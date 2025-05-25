using MongoDB.Driver;
using vendingmachines.commands.domain.DDD;
using vendingmachines.commands.persistence.Datamodels;

namespace vendingmachines.commands.persistence.Repository;

public interface IEventsRepository : IRepository
{
    Task<EventsDataModel> Save(EventsDataModel eventDm, IClientSessionHandle? session = null);
    Task<List<EventsDataModel>> FindByAggId(string aggId);
    Task<IEnumerable<EventsDataModel>> FindAll();
}
