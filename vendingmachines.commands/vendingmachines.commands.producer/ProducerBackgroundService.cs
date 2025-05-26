using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using vendingmachines.commands.persistence.Repository;

namespace vendingmachines.commands.producer;

public class ProducerBackgroundService(ILogger<ProducerBackgroundService> logger, EventProducerLogic eventProducerLogic, 
    IOutboxRepository outboxRepository) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var odmsNotProcessed = await outboxRepository.FindAllNotProcessed();

            if (odmsNotProcessed.Count > 0)
            {
                logger.LogInformation("Found {} messages not yet produced", odmsNotProcessed.Count);
            }

            foreach (var odm in odmsNotProcessed)
            {
                await eventProducerLogic.ProduceMessage(odm.BaseDomainEvent);
                await outboxRepository.UpdateOutboxEntry(odm);
            }

            await Task.Delay(5000, stoppingToken);
        }
    }
}
