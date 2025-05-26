using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace vendingmachines.queries.consumers;

public abstract class BaseConsumer<TMessage> : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BaseConsumer<TMessage>> _logger;

    protected BaseConsumer(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<BaseConsumer<TMessage>> logger)
    {
        _serviceProvider = serviceProvider;

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            GroupId = configuration["Kafka:GroupId"],
            AutoOffsetReset = AutoOffsetReset.Latest,
            EnableAutoCommit = false,
        };

        _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        _logger = logger;
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
        _logger.LogInformation("Subscribing to {}", topic);

        try
        {
            _consumer.Subscribe(topic);

            var msg = _consumer.Consume(stoppingToken);
            if (msg == null) return;

            _logger.LogInformation("Message received: {}", msg.Message.Value);
            var message = JsonSerializer.Deserialize<TMessage>(msg.Message.Value);
            if (message == null) return;

            using var scope = _serviceProvider.CreateScope();
            await HandleMessageAsync(message, scope.ServiceProvider, stoppingToken, _consumer);
        }
        catch (Exception e)
        {
            _logger.LogError("An error occoured. Message: {}", e.Message);
            throw;
        }
    }
    protected abstract Task HandleMessageAsync(TMessage message, IServiceProvider serviceProvider, CancellationToken stoppingToken, IConsumer<string, string> _consumer);
    protected abstract string GetTopic();
}