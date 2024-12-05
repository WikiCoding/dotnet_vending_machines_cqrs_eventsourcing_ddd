using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using vendingmachines.queries.contracts;
using vendingmachines.queries.repository;

namespace vendingmachines.queries.consumers;

public class ProductOrderedTopicConsumer : BaseConsumer<ProductOrderedMessage>
{
    public ProductOrderedTopicConsumer(IConfiguration configuration, IServiceProvider serviceProvider) : base(configuration, serviceProvider)
    {
    }

    protected override string GetTopic()
    {
        return "product-ordered-topic";
    }

    protected override async Task HandleMessageAsync(ProductOrderedMessage message, IServiceProvider serviceProvider, CancellationToken stoppingToken)
    {
        var dbContext = serviceProvider.GetRequiredService<AppDbContext>();

        var product = await dbContext.Products.Where(p => p.ProductId.Equals(message.ProductId)).FirstOrDefaultAsync(stoppingToken);

        if (product == null) throw new NullReferenceException("Product not found");

        product.ProductQty -= message.OrderedQty;

        var rowsAffected = await dbContext.Products
            .Where(product => product.ProductId == message.ProductId)
            .ExecuteUpdateAsync(product => product
                .SetProperty(p => p.ProductQty, p => p.ProductQty - message.OrderedQty), stoppingToken);

        Console.WriteLine(rowsAffected == 1 ? "Ppdated qty of product after order" : "Product update failed");
    }
}
