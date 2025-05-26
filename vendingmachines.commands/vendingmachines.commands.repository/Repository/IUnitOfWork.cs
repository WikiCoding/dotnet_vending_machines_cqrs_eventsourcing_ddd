using vendingmachines.commands.domain.DomainEvents;
using vendingmachines.commands.persistence.Datamodels;

namespace vendingmachines.commands.persistence.Repository;

public interface IUnitOfWork
{
    Task AtomicSave(EventsDataModel eventDm, BaseDomainEvent evnt);
}
