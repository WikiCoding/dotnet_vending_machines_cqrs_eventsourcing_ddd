using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace vendingmachines.queries.consumers;

public abstract class BaseConsumer<TMessage> : BackgroundService
{
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly IServiceProvider _serviceProvider;

    protected BaseConsumer(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            GroupId = configuration["Kafka:GroupId"],
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

    protected async Task Consume(CancellationToken stoppingToken)
    {
        var topic = GetTopic();
        Console.WriteLine($"Subscribing to {topic}");
        _consumer.Subscribe(topic);

        var msg = _consumer.Consume(stoppingToken);
        if (msg == null) return;

        Console.WriteLine($"Message received: {msg.Message.Value}");
        var message = JsonSerializer.Deserialize<TMessage>(msg.Message.Value);
        if (message == null) return;

        using var scope = _serviceProvider.CreateScope();
        await HandleMessageAsync(message, scope.ServiceProvider, stoppingToken);
    }
    protected abstract Task HandleMessageAsync(TMessage message, IServiceProvider serviceProvider, CancellationToken stoppingToken);
    protected abstract string GetTopic();
}