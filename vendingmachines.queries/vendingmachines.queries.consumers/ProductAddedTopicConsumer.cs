using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using vendingmachines.queries.contracts;
using vendingmachines.queries.entities;
using vendingmachines.queries.repository;

namespace vendingmachines.queries.consumers;

public class ProductAddedTopicConsumer(IConfiguration configuration, IServiceProvider serviceProvider)
    : BaseConsumer<ProductAddedMessage>(configuration, serviceProvider)
{
    protected override async Task HandleMessageAsync(ProductAddedMessage message, IServiceProvider serviceProvider, CancellationToken stoppingToken)
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

        Console.WriteLine($"Product added to database: {product.ProductName}");
    }

    protected override string GetTopic()
    {
        return "product-added-topic";
    }
}
