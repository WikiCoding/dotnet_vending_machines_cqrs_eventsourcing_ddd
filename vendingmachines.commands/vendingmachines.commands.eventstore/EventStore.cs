using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using vendingmachines.commands.domain.DomainEvents;
using vendingmachines.commands.domain.Entites;
using vendingmachines.commands.domain.ValueObjects;
using vendingmachines.commands.persistence.Datamodels;
using vendingmachines.commands.persistence.Repository;
using vendingmachines.commands.producer;

namespace vendingmachines.commands.eventstore;

public class EventStore(ILogger<EventStore> logger, ISnapshotRepository snapshotsRepository, IEventsRepository eventsRepository, IUnitOfWork unitOfWork,
    EventProducerLogic eventProducerLogic)
{
    public async Task<List<BaseDomainEvent>> GetEventsByAggregateId(string aggId)
    {
        logger.LogInformation("No snapshots for aggregate id {}. Getting all events from db to rebuild state", aggId);

        var events = await eventsRepository.FindByAggId(aggId);

        if (events.Count == 0) throw new Exception("Events with that aggregate id were not found");
        
        return events.Select(e => e.DomainEvent).ToList();
    }

    public async Task<Machine?> GetAggregateFromSnapshot(string aggregateId)
    {
        var machineSnapshot = await snapshotsRepository.FindByAggId(aggregateId);

        if (machineSnapshot is null) return null;

        var machineAgg = new Machine();

        var machineId = new MachineId(machineSnapshot.AggregateId);
        var machineType = new MachineType(machineSnapshot.MachineType);
        var machineVersion = machineSnapshot.Version;
        var productsAtSnapshot = machineSnapshot.products.Select(p =>
        {
            var productId = new ProductId(p.ProductId);
            var productName = new ProductName(p.ProductName);
            var productQty = new ProductQty(p.ProductQty);

            return new Product(productId, productName, productQty);
        }).ToList();

        machineAgg.ToSnapshot(machineId, machineType, machineVersion, productsAtSnapshot);

        logger.LogInformation("Aggregate with Id {} found in the snapshots db", aggregateId);

        return machineAgg;
    }

    public async Task SaveEvents(string aggId, string machineType, IReadOnlyList<BaseDomainEvent> events, int expectedVersion, IReadOnlyList<Product> products)
    {
        var latestSnapshot = await GetAggregateFromSnapshot(aggId);
        var version = 0;

        if (latestSnapshot is not null)
        {
            logger.LogInformation("Found snapshot for aggregate id {} with version {}", aggId, latestSnapshot.Version);

            if (latestSnapshot.Version >= expectedVersion)
            {
                logger.LogError("Concurreny Exception");
                throw new Exception("Concurrency exception");
            }

            version = latestSnapshot.Version;
        } else
        {
            logger.LogInformation("Getting all events from db for aggregate id {}", aggId);
            var eventStream = await eventsRepository.FindByAggId(aggId);

            if (eventStream.Count > 0 && eventStream.Last().Version >= expectedVersion)
            {
                logger.LogError("Concurreny Exception");
                throw new Exception("Concurrency exception");
            }

            version = expectedVersion;
        }

        foreach (var evnt in events)
        {
            evnt.Version = version;

            var eventDm = new EventsDataModel
            {
                AggregateId = evnt.AggregateId,
                DomainEvent = evnt,
                CreatedAt = evnt.CreatedAt,
                Version = version,
            };

            version++;

            await unitOfWork.AtomicSave(eventDm, evnt);
        }

        // TODO: this is something to think and improve on... I'm not happy with the way the snapshots are persisted
        await PersistSnapshot(aggId, machineType, version, products);
    }

    public async Task RebuildReadDb()
    {
        var events = await eventsRepository.FindAll();

        foreach (var evnt in events)
        {
            await eventProducerLogic.ProduceMessage(evnt.DomainEvent);
        }
    }

    private async Task PersistSnapshot(string aggId, string machineType, int expectedVersion, IReadOnlyList<Product> products)
    {
        var snapshotDataModel = GenerateSnapshotDataModel(aggId, machineType, expectedVersion, products);
        await snapshotsRepository.SaveSnapshot(snapshotDataModel);
    }

    private SnapshotDataModel GenerateSnapshotDataModel(string aggId, string machineType, int expectedVersion, IReadOnlyList<Product> products)
    {
        var productsDm = products.Select(p => new ProductSnapshotDataModel
        {
            ProductId = p.ProductId.Id,
            ProductName = p.ProductName.Name,
            ProductQty = p.ProductQty.qty
        }
        ).ToList();

        return new SnapshotDataModel
        {
            AggregateId = Guid.Parse(aggId),
            MachineType = machineType,
            products = productsDm,
            Version = expectedVersion
        };
    }
}
