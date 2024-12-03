using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace vendingmachines.queries.consumers;

public class ProductQtyUpdatedTopicConsumer : BackgroundService
{
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;

    public ProductQtyUpdatedTopicConsumer(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"],
            GroupId = _configuration["Kafka:GroupId"],
            AutoOffsetReset = AutoOffsetReset.Earliest
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
        const string topic = "product-qty-updated-topic";
        Console.WriteLine($"Subscribing to {topic}");

        _consumer.Subscribe(topic);

        var msg = _consumer.Consume(cancellationToken);

        if (msg == null) return;

        var msgValue = msg.Message.Value;

        Console.WriteLine($"Message received: {msgValue}");

        //var orderCreated = JsonSerializer.Deserialize<OrderCreatedEvent>(msgValue);

        //if (orderCreated == null) return;

        //_logger.LogWarning("Received OrderCreated message for OrderId: {OrderId}", orderCreated.orderId);

        //using var scope = _serviceProvider.CreateScope();
        //var dbContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

        //_logger.LogWarning("Saving the new Order with id {OrderId} in the Orders Table", orderCreated.orderId);

        //var order = new OrderDataModel
        //{
        //    OrderId = orderCreated.orderId,
        //    OrderQty = orderCreated.orderQty,
        //    ProductId = orderCreated.productId,
        //    OrderStatus = OrderStatus.PENDING_CONFIRMATION,
        //    LastUpdatedAt = DateTime.UtcNow.ToString()
        //};

        //dbContext.Orders.Add(order);

        //await dbContext.SaveChangesAsync(cancellationToken);
    }
}
