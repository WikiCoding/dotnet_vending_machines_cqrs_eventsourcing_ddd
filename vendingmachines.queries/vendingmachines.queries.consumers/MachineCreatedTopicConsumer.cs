using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using vendingmachines.queries.contracts;
using vendingmachines.queries.entities;
using vendingmachines.queries.repository;

namespace vendingmachines.queries.consumers;

public class MachineCreatedTopicConsumer(IConfiguration configuration, IServiceProvider serviceProvider)
    : BaseConsumer<MachineCreatedMessage>(configuration, serviceProvider)
{
    protected override async Task HandleMessageAsync(MachineCreatedMessage message, IServiceProvider serviceProvider,
        CancellationToken stoppingToken)
    {
        var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
        var machine = new Machine
        {
            MachineId = message.AggregateId,
            MachineType = message.MachineType
        };

        dbContext.Machines.Add(machine);
        await dbContext.SaveChangesAsync(stoppingToken);
        Console.WriteLine("Machine saved in db...");
        // Commit offsets here if needed
    }

    protected override string GetTopic()
    {
        return "machine-created-topic";
    }
}