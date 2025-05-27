using Microsoft.Extensions.Logging;
using System.Text.Json;
using vendingmachines.commands.domain.DomainEvents;

namespace vendingmachines.commands.producer;

public class EventProducerLogic(ILogger<EventProducerLogic> logger, KafkaProducer kafkaProducer)
{
    public async Task ProduceMessage(BaseDomainEvent evnt)
    {
        var topic = "machines-topic";
        var message = "";

        if (evnt is MachineCreatedEvent) message = JsonSerializer.Serialize((MachineCreatedEvent)evnt);
        if (evnt is ProductAddedEvent) message = JsonSerializer.Serialize((ProductAddedEvent)evnt);
        if (evnt is ProductQtyUpdatedEvent) message = JsonSerializer.Serialize((ProductQtyUpdatedEvent)evnt);
        if (evnt is ProductOrderedEvent) message = JsonSerializer.Serialize((ProductOrderedEvent)evnt);

        if (string.IsNullOrEmpty(topic) || string.IsNullOrEmpty(message))
        {
            logger.LogInformation("No message is produced");
            return;
        }

        await kafkaProducer.ProduceAsync(topic, evnt.AggregateId, message, CancellationToken.None);
    }
}
