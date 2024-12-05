using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using vendingmachines.queries.contracts;
using vendingmachines.queries.repository;

namespace vendingmachines.queries.consumers;

public class ProductQtyUpdatedTopicConsumer(IConfiguration configuration, IServiceProvider serviceProvider)
    : BaseConsumer<ProductQtyUpdatedMessage>(configuration, serviceProvider)
{
    protected override async Task HandleMessageAsync(ProductQtyUpdatedMessage message, IServiceProvider serviceProvider,
        CancellationToken stoppingToken)
    {
        var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
        
        var product = await dbContext.Products.Where(p => p.ProductId.Equals(message.ProductId)).FirstOrDefaultAsync(stoppingToken);
        
        if (product == null) throw new NullReferenceException("Product not found");

        product.ProductQty = message.ProductQty;
        
        var rowsAffected = await dbContext.Products
            .Where(product => product.ProductId == message.ProductId)
            .ExecuteUpdateAsync(product => product
                .SetProperty(p => p.ProductQty, p => message.ProductQty), cancellationToken: stoppingToken);

        Console.WriteLine(rowsAffected == 1 ? "Product qty updated" : "Product update failed");
    }

    protected override string GetTopic()
    {
        return "product-qty-updated-topic";
    }
}
