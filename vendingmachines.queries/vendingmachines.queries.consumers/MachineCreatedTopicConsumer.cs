using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using vendingmachines.queries.contracts;
using vendingmachines.queries.entities;
using vendingmachines.queries.repository;

namespace vendingmachines.queries.consumers;

public class MachineCreatedTopicConsumer(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<MachineCreatedTopicConsumer> logger)
    : BaseConsumer<MachineCreatedMessage>(configuration, serviceProvider, logger)
{
    protected override async Task HandleMessageAsync(MachineCreatedMessage message, IServiceProvider serviceProvider,
        CancellationToken stoppingToken, IConsumer<string, string> _consumer)
    {
        var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
        var machine = new Machine
        {
            MachineId = message.AggregateId,
            MachineType = message.MachineType
        };

        dbContext.Machines.Add(machine);
        await dbContext.SaveChangesAsync(stoppingToken);
        logger.LogInformation("Machine saved in db, commiting offsets");
        
        _consumer.Commit();
    }

    protected override string GetTopic()
    {
        return "machine-created-topic";
    }
}