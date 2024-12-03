using System.Text.Json;
using vendingmachines.commands.domain.DomainEvents;
using vendingmachines.commands.persistence.Datamodels;
using vendingmachines.commands.persistence.Repository;
using vendingmachines.commands.producer;

namespace vendingmachines.commands.eventstore;

public class EventStore
{
    private readonly IEventsRepository _eventsRepository;
    private readonly KafkaProducer _kafkaProducer;

    public EventStore(IEventsRepository eventsRepository, KafkaProducer kafkaProducer)
    {
        _eventsRepository = eventsRepository;
        _kafkaProducer = kafkaProducer;
    }

    public async Task<List<BaseDomainEvent>> GetEventsByAggregateId(string aggId)
    {
        var events = await _eventsRepository.FindByAggId(aggId);

        if (events.Count == 0) throw new Exception("Events with that aggregate id were not found");
        
        return events.Select(e => e.DomainEvent).ToList();
    }

    public async Task SaveEvents(string aggId, IReadOnlyList<BaseDomainEvent> events, int expectedVersion)
    {
        var eventStream = await _eventsRepository.FindByAggId(aggId);
        if (eventStream.Count > 0 && eventStream.Last().Version >= expectedVersion)
        {
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

        if (string.IsNullOrEmpty(topic) || string.IsNullOrEmpty(message))
        {
            Console.WriteLine("No message is produced");
            return;
        }

        await _kafkaProducer.ProduceAsync(topic, message, CancellationToken.None);
    }
}
