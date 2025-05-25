using Microsoft.Extensions.Logging;
using System.Text.Json;
using vendingmachines.commands.domain.DomainEvents;
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

    public EventStore(ILogger<EventStore> logger, IEventsRepository eventsRepository, KafkaProducer kafkaProducer, SnapshotsRepository snapshotsRepository)
    {
        _eventsRepository = eventsRepository;
        _kafkaProducer = kafkaProducer;
        _logger = logger;
        _snapshotsRepository = snapshotsRepository;
    }

    public async Task<List<BaseDomainEvent>> GetEventsByAggregateId(string aggId)
    {
        var eventsSnapshotted = await _snapshotsRepository.FindByAggId(aggId);

        if (eventsSnapshotted is not null && eventsSnapshotted.Count > 0)
        {
            _logger.LogInformation("Found snapshot for aggregate id {}", aggId);
            return eventsSnapshotted.Select(e => e.DomainEvent).ToList();
        }

        _logger.LogInformation("No snapshots for aggregate id {}. Getting all events from db to rebuild state", aggId);

        var events = await _eventsRepository.FindByAggId(aggId);

        if (events.Count == 0) throw new Exception("Events with that aggregate id were not found");
        
        return events.Select(e => e.DomainEvent).ToList();
    }

    public async Task SaveEvents(string aggId, IReadOnlyList<BaseDomainEvent> events, int expectedVersion)
    {
        List<EventsDataModel> eventStream;
        var eventsSnapshotted = await _snapshotsRepository.FindByAggId(aggId);

        if (eventsSnapshotted is not null && eventsSnapshotted.Count > 0)
        {
            _logger.LogInformation("Found snapshot for aggregate id {}", aggId);
            eventStream = eventsSnapshotted;
        }
        else
        {
            _logger.LogInformation("Getting all events from db for aggregate id {}", aggId);
            eventStream = await _eventsRepository.FindByAggId(aggId);
        }

        if (eventStream.Count > 0 && eventStream.Last().Version >= expectedVersion)
        {
            _logger.LogError("Concurreny Exception");
            throw new Exception("Concurrency exception");
        }

        var version = expectedVersion;

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

            await _snapshotsRepository.Save(eventDm);
            await _eventsRepository.Save(eventDm);

            // we can produce the message here
            await ProduceMessage(evnt);
        }
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

        await _kafkaProducer.ProduceAsync(topic, message, CancellationToken.None);
    }

    public async Task RebuildReadDb()
    {
        var events = await _eventsRepository.FindAll();

        foreach (var evnt in events)
        {
            await ProduceMessage(evnt.DomainEvent);
        }
    }
}
