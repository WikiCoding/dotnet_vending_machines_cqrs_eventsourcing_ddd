using Microsoft.Extensions.Logging;
using vendingmachines.commands.domain.Entites;
using vendingmachines.commands.domain.ValueObjects;
using vendingmachines.commands.eventstore;
using vendingmachines.commands.persistence.Datamodels;
using vendingmachines.commands.persistence.Repository;

namespace vendingmachines.commands.eventsourcinghandler;

public class EventSourcingHandler
{
    private readonly EventStore _eventStore;
    private readonly SnapshotsRepository _snapshotsRepository;
    private readonly ILogger<EventSourcingHandler> _logger;

    public EventSourcingHandler(ILogger<EventSourcingHandler> logger, EventStore eventStore, SnapshotsRepository snapshotsRepository)
    {
        _eventStore = eventStore;
        _snapshotsRepository = snapshotsRepository;
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

    private async Task PersistSnapshot(Machine machine)
    {
        var snapshotDataModel = GenerateSnapshotDataModel(machine);
        await _snapshotsRepository.SaveSnapshot(snapshotDataModel);
    }

    private SnapshotDataModel GenerateSnapshotDataModel(Machine machine)
    {
        var productsDm = machine.GetProducts().Select(p => new ProductSnapshotDataModel { 
            ProductId = p.ProductId.Id,
            ProductName = p.ProductName.Name,
            ProductQty = p.ProductQty.qty }
        ).ToList();

        return new SnapshotDataModel
        {
            AggregateId = machine.MachineId.Id,
            MachineType = machine.MachineType.Type,
            products = productsDm,
            Version = machine.Version
        };
    }

    public async Task RebuildQueriesDbState()
    {
        await _eventStore.RebuildReadDb();
    }
}
