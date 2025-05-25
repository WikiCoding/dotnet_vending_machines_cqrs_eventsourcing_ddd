using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Text.Json;
using vendingmachines.commands.domain.DomainEvents;
using vendingmachines.commands.domain.Entites;
using vendingmachines.commands.domain.ValueObjects;
using vendingmachines.commands.persistence.Datamodels;
using vendingmachines.commands.persistence.Repository;
using vendingmachines.commands.producer;

namespace vendingmachines.commands.eventstore;

public class EventStore
{
    private readonly IEventsRepository _eventsRepository;
    private readonly SnapshotsRepository _snapshotsRepository;
    private readonly KafkaProducer _kafkaProducer;
    private readonly ILogger<EventStore> _logger;
    private readonly IMongoClient _mongoClient;

    public EventStore(ILogger<EventStore> logger, IEventsRepository eventsRepository, KafkaProducer kafkaProducer, SnapshotsRepository snapshotsRepository, IMongoClient mongoClient)
    {
        _eventsRepository = eventsRepository;
        _kafkaProducer = kafkaProducer;
        _logger = logger;
        _snapshotsRepository = snapshotsRepository;
        _mongoClient = mongoClient;
    }

    public async Task<List<BaseDomainEvent>> GetEventsByAggregateId(string aggId)
    {
        _logger.LogInformation("No snapshots for aggregate id {}. Getting all events from db to rebuild state", aggId);

        var events = await _eventsRepository.FindByAggId(aggId);

        if (events.Count == 0) throw new Exception("Events with that aggregate id were not found");
        
        return events.Select(e => e.DomainEvent).ToList();
    }

    public async Task<Machine?> GetAggregateFromSnapshot(string aggregateId)
    {
        var machineSnapshot = await _snapshotsRepository.FindByAggId(aggregateId);

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

        _logger.LogInformation("Aggregate with Id {} found in the snapshots db", aggregateId);

        return machineAgg;
    }

    public async Task SaveEvents(string aggId, string machineType, IReadOnlyList<BaseDomainEvent> events, int expectedVersion, IReadOnlyList<Product> products)
    {
        var latestSnapshot = await GetAggregateFromSnapshot(aggId);
        var version = 0;

        if (latestSnapshot is not null)
        {
            _logger.LogInformation("Found snapshot for aggregate id {} with version {}", aggId, latestSnapshot.Version);

            if (latestSnapshot.Version >= expectedVersion)
            {
                _logger.LogError("Concurreny Exception");
                throw new Exception("Concurrency exception");
            }

            version = latestSnapshot.Version;
        } else
        {
            _logger.LogInformation("Getting all events from db for aggregate id {}", aggId);
            var eventStream = await _eventsRepository.FindByAggId(aggId);

            if (eventStream.Count > 0 && eventStream.Last().Version >= expectedVersion)
            {
                _logger.LogError("Concurreny Exception");
                throw new Exception("Concurrency exception");
            }

            version = expectedVersion;
        }

        using var session = await _mongoClient.StartSessionAsync();
        session.StartTransaction();

        try
        {
            await ProcessEventsProduceMessagesAndPersistSnapshot(aggId, machineType, events, version, products, session);
        }
        catch (Exception ex)
        {
            await session.AbortTransactionAsync();
            _logger.LogError("Transaction failed: {}", ex.Message);
            throw;
        }
    }

    public async Task ProcessEventsProduceMessagesAndPersistSnapshot(string aggId, string machineType, IReadOnlyList<BaseDomainEvent> events, 
        int version, IReadOnlyList<Product> products, IClientSessionHandle session)
    {
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

            await _eventsRepository.Save(eventDm, session);

            // we can produce the message here
            await ProduceMessage(evnt);
        }

        await PersistSnapshot(aggId, machineType, version, products);

        await session.CommitTransactionAsync();
    }

    private async Task ProduceMessage(BaseDomainEvent evnt)
    {
        var topic = "";
        var message = "";

        if (evnt is MachineCreatedEvent)
        {
            topic = "machine-created-topic";
            message = JsonSerializer.Serialize((MachineCreatedEvent)evnt);
        }

        if (evnt is ProductAddedEvent)
        {
            topic = "product-added-topic";
            message = JsonSerializer.Serialize((ProductAddedEvent)evnt);
        }

        if (evnt is ProductQtyUpdatedEvent)
        {
            topic = "product-qty-updated-topic";
            message = JsonSerializer.Serialize((ProductQtyUpdatedEvent)evnt);
        }

        if (evnt is ProductOrderedEvent)
        {
            topic = "product-ordered-topic";
            message = JsonSerializer.Serialize((ProductOrderedEvent)evnt);
        }

        if (string.IsNullOrEmpty(topic) || string.IsNullOrEmpty(message))
        {
            _logger.LogInformation("No message is produced");
            return;
        }

        // TODO: Replace with the outbox pattern to ensure to data correctness or with Kafka Connect CDC
        await _kafkaProducer.ProduceAsync(topic, evnt.AggregateId, message, CancellationToken.None);
    }

    public async Task RebuildReadDb()
    {
        var events = await _eventsRepository.FindAll();

        foreach (var evnt in events)
        {
            await ProduceMessage(evnt.DomainEvent);
        }
    }

    private async Task PersistSnapshot(string aggId, string machineType, int expectedVersion, IReadOnlyList<Product> products)
    {
        var snapshotDataModel = GenerateSnapshotDataModel(aggId, machineType, expectedVersion, products);
        await _snapshotsRepository.SaveSnapshot(snapshotDataModel);
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
