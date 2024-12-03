using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using vendingmachines.queries.contracts;
using vendingmachines.queries.entities;
using vendingmachines.queries.repository;

namespace vendingmachines.queries.consumers;

public class MachineCreatedTopicConsumer : BackgroundService
{
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;

    public MachineCreatedTopicConsumer(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"],
            GroupId = _configuration["Kafka:GroupId"],
            AutoOffsetReset = AutoOffsetReset.Earliest
            // TODO: set auto commit offsets off!
        };

        _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            await Consume(stoppingToken);

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }

        _consumer.Close();
    }

    private async Task Consume(CancellationToken cancellationToken)
    {
        const string topic = "machine-created-topic";
        Console.WriteLine($"Subscribing to {topic}");

        _consumer.Subscribe(topic);

        var msg = _consumer.Consume(cancellationToken);

        if (msg == null) return;

        var msgValue = msg.Message.Value;

        Console.WriteLine($"Message received: {msgValue}");

        var machineCreated = JsonSerializer.Deserialize<MachineCreatedMessage>(msgValue);

        if (machineCreated == null) return;

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var machine = new Machine
        {
            MachineId = machineCreated.AggregateId,
            MachineType = machineCreated.MachineType
        };

        dbContext.Machines.Add(machine);

        await dbContext.SaveChangesAsync(cancellationToken);

        Console.WriteLine("Machine saved in db...");
        
        // commit offsets now!
    }
}
