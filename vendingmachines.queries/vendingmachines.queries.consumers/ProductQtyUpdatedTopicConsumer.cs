using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using vendingmachines.queries.contracts;
using vendingmachines.queries.repository;

namespace vendingmachines.queries.consumers;

public class ProductQtyUpdatedTopicConsumer : BaseConsumer<ProductQtyUpdatedMessage>
{
    public ProductQtyUpdatedTopicConsumer(IConfiguration configuration, IServiceProvider serviceProvider) : base(configuration, serviceProvider)
    {
    }

    protected override async Task HandleMessageAsync(ProductQtyUpdatedMessage message, IServiceProvider serviceProvider,
        CancellationToken stoppingToken)
    {
        var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
        
        var product = await dbContext.Products.Where(p => p.ProductId.Equals(message.ProductId)).FirstOrDefaultAsync(stoppingToken);
        
        if (product == null) throw new NullReferenceException("Product not found");

        product.ProductQty = message.ProductQty;
        
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(stoppingToken);

        Console.WriteLine("Product updated qty updated");
    }

    protected override string GetTopic()
    {
        return "product-qty-updated-topic";
    }
}
