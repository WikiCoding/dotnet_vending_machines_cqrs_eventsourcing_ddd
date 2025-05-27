using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using vendingmachines.queries.contracts;
using vendingmachines.queries.entities;
using vendingmachines.queries.repository;

namespace vendingmachines.queries.consumers;

public class MachineCreatedMessageHandler(IServiceProvider serviceProvider, ILogger<MachineCreatedMessageHandler> logger)
{
    public async Task HandleMachineCreatedEvent(string message, IConsumer<string, string> consumer)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var deserialized = JsonSerializer.Deserialize<MachineCreatedMessage>(message);
        if (deserialized is null)
        {
            logger.LogError("Message wasn't deserialized properly");
            throw new JsonException("Could not deserialize message");
        }
        var machine = new Machine
        {
            MachineId = deserialized.AggregateId,
            MachineType = deserialized.MachineType
        };

        dbContext.Machines.Add(machine);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Machine saved in db, commiting offsets");

        consumer.Commit();
    }
}