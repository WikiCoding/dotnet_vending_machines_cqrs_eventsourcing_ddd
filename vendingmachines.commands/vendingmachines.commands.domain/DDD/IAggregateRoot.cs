using vendingmachines.commands.domain.DomainEvents;

namespace vendingmachines.commands.domain.DDD;

public interface IAggregateRoot
{
    IReadOnlyList<BaseDomainEvent> GetEvents();
    void EventsCommited();
}
