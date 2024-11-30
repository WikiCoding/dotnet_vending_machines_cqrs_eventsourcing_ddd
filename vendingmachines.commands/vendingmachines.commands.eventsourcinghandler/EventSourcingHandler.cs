using vendingmachines.commands.domain.Entites;
using vendingmachines.commands.eventstore;

namespace vendingmachines.commands.eventsourcinghandler;

public class EventSourcingHandler
{
    private readonly EventStore _eventStore;

    public EventSourcingHandler(EventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task Save(Machine machine)
    {
        await _eventStore.SaveEvents(machine.MachineId.Id.ToString(), machine.GetEvents(), machine.Version);
        machine.EventsCommited();
    }

    public async Task<Machine> GetAggregateById(string aggregateId)
    {
        var aggEvents = await _eventStore.GetEventsByAggregateId(aggregateId);
        var machine = new Machine();
        machine.RebuildState(aggEvents);

        return machine;
    }
}
