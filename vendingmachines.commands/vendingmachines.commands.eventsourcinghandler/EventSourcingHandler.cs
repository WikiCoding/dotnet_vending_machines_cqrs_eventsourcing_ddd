using Microsoft.Extensions.Logging;
using vendingmachines.commands.domain.Entites;
using vendingmachines.commands.eventstore;

namespace vendingmachines.commands.eventsourcinghandler;

public class EventSourcingHandler
{
    private readonly EventStore _eventStore;
    private readonly ILogger<EventSourcingHandler> _logger;

    public EventSourcingHandler(ILogger<EventSourcingHandler> logger, EventStore eventStore)
    {
        _eventStore = eventStore;
        _logger = logger;
    }

    public async Task Save(Machine machine)
    {
        await _eventStore.SaveEvents(machine.MachineId.Id.ToString(), machine.MachineType.Type, machine.GetEvents(), machine.Version, machine.GetProducts());
        machine.EventsCommited();
    }

    public async Task<Machine> GetAggregateById(string aggregateId)
    {
        Machine? machineFromSnapshot = await _eventStore.GetAggregateFromSnapshot(aggregateId);

        if (machineFromSnapshot is not null) return machineFromSnapshot;

        _logger.LogInformation("Snapshot with Id {} not found rebuilding state", aggregateId);

        var aggEvents = await _eventStore.GetEventsByAggregateId(aggregateId);
        var machine = new Machine();
        machine.RebuildState(aggEvents);

        return machine;
    }

    public async Task RebuildQueriesDbState()
    {
        await _eventStore.RebuildReadDb();
    }
}
