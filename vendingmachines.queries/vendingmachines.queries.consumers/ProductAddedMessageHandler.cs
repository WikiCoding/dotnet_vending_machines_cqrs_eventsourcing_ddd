using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using vendingmachines.queries.contracts;
using vendingmachines.queries.entities;
using vendingmachines.queries.repository;

namespace vendingmachines.queries.consumers;

public class ProductAddedMessageHandler(IServiceProvider serviceProvider, ILogger<ProductAddedMessageHandler> logger)
{
    public async Task HandleProductAddedEvent(string message, IConsumer<string, string> consumer)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var deserialized = JsonSerializer.Deserialize<ProductAddedMessage>(message);

        if (deserialized is null)
        {
            logger.LogError("Message wasn't deserialized properly");
            throw new JsonException("Could not deserialize message");
        }

        var product = new Product
        {
            MachineId = deserialized.AggregateId,
            ProductId = deserialized.ProductId,
            ProductName = deserialized.ProductName,
            ProductQty = deserialized.ProductQty
        };

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Product added to database: {}", product.ProductName);

        consumer.Commit();
    }
}
