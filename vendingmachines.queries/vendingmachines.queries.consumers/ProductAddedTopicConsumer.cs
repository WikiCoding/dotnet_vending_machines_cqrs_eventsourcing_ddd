using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using vendingmachines.queries.contracts;
using vendingmachines.queries.entities;
using vendingmachines.queries.repository;

namespace vendingmachines.queries.consumers;

public class ProductAddedTopicConsumer(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<ProductAddedTopicConsumer> logger)
    : BaseConsumer<ProductAddedMessage>(configuration, serviceProvider, logger)
{
    protected override async Task HandleMessageAsync(ProductAddedMessage message, IServiceProvider serviceProvider, CancellationToken stoppingToken, 
        IConsumer<string, string> _consumer)
    {
        var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
        var product = new Product
        {
            MachineId = message.AggregateId,
            ProductId = message.ProductId,
            ProductName = message.ProductName,
            ProductQty = message.ProductQty
        };
        
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(stoppingToken);

        logger.LogInformation("Product added to database: {}", product.ProductName);

        _consumer.Commit();
    }

    protected override string GetTopic()
    {
        return "product-added-topic";
    }
}
