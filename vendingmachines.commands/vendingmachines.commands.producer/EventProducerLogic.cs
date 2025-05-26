using Microsoft.Extensions.Logging;
using System.Text.Json;
using vendingmachines.commands.domain.DomainEvents;

namespace vendingmachines.commands.producer;

public class EventProducerLogic(ILogger<EventProducerLogic> logger, KafkaProducer kafkaProducer)
{
    public async Task ProduceMessage(BaseDomainEvent evnt)
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
            logger.LogInformation("No message is produced");
            return;
        }

        await kafkaProducer.ProduceAsync(topic, evnt.AggregateId, message, CancellationToken.None);
    }
}
