using vendingmachines.commands.domain.Entites;
using vendingmachines.commands.eventstore;
using vendingmachines.commands.persistence.Datamodels;

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
        // TODO: now with the snapshots, since I'm not storing the whole aggregate at once, I can't rebuild the state correctly... to fix
        var machine = new Machine();
        machine.RebuildState(aggEvents);

        return machine;
    }

    public async Task RebuildQueriesDbState()
    {
        await _eventStore.RebuildReadDb();
    }
}
