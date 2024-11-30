using vendingmachines.commands.domain.DDD;
using vendingmachines.commands.domain.Entites;
using vendingmachines.commands.persistence.Datamodels;

namespace vendingmachines.commands.persistence.Repository;

public interface IEventsRepository : IRepository
{
    Task<EventsDataModel> SaveMachine(Machine machine);
}
