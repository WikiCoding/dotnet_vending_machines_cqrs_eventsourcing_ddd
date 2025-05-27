using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace vendingmachines.queries.consumers;

public class DbUpdatesConsumer : BackgroundService
{
    private const string _topic = "machines-topic";
    private readonly IConsumer<string, string> _consumer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DbUpdatesConsumer> _logger;
    private readonly MachineCreatedMessageHandler _machineCreatedMessageHandler;
    private readonly ProductAddedMessageHandler _productAddedMessageHandler;
    private readonly ProductQtyUpdatedMessageHandler _productQtyUpdatedMessageHandler;
    private readonly ProductOrderedMessageHandler _productOrderedMessageHandler;

    public DbUpdatesConsumer(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<DbUpdatesConsumer> logger,
        MachineCreatedMessageHandler machineCreatedMessageHandler, ProductAddedMessageHandler productAddedMessageHandler, 
        ProductQtyUpdatedMessageHandler productQtyUpdatedMessageHandler, ProductOrderedMessageHandler productOrderedMessageHandler)
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
        _machineCreatedMessageHandler = machineCreatedMessageHandler;
        _productAddedMessageHandler = productAddedMessageHandler;
        _productQtyUpdatedMessageHandler = productQtyUpdatedMessageHandler;
        _productOrderedMessageHandler = productOrderedMessageHandler;
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
        _logger.LogInformation("Subscribing to {}", _topic);

        try
        {
            _consumer.Subscribe(_topic);

            var msg = _consumer.Consume(stoppingToken);
            if (msg == null) return;

            _logger.LogInformation("Message received: {}", msg.Message.Value);

            await DeserializeAndPersistMessage(msg.Message.Value);
        }
        catch (Exception e)
        {
            _logger.LogError("An error occoured. Message: {}", e.Message);
            throw;
        }
    }

    public async Task DeserializeAndPersistMessage(string message)
    {
        if (message.Contains("CreateMachineCommand")) await _machineCreatedMessageHandler.HandleMachineCreatedEvent(message, _consumer);
        if (message.Contains("ProductAddedEvent")) await _productAddedMessageHandler.HandleProductAddedEvent(message, _consumer);
        if (message.Contains("ProductQtyUpdatedEvent")) await _productQtyUpdatedMessageHandler.HandleProductQtyUpdatedEvent(message, _consumer);
        if (message.Contains("ProductOrderedEvent")) await _productOrderedMessageHandler.HandleProductOrdered(message, _consumer);
    }
}
