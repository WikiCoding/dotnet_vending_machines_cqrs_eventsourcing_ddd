using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Transactions;
using vendingmachines.queries.contracts;
using vendingmachines.queries.repository;

namespace vendingmachines.queries.consumers;

public class ProductQtyUpdatedTopicConsumer(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<ProductQtyUpdatedTopicConsumer> logger)
    : BaseConsumer<ProductQtyUpdatedMessage>(configuration, serviceProvider, logger)
{
    protected override async Task HandleMessageAsync(ProductQtyUpdatedMessage message, IServiceProvider serviceProvider,
        CancellationToken stoppingToken, IConsumer<string, string> _consumer)
    {
        var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
        
        var product = await dbContext.Products.Where(p => p.ProductId.Equals(message.ProductId)).FirstOrDefaultAsync(stoppingToken);
        
        if (product == null) throw new NullReferenceException("Product not found");

        product.ProductQty = message.ProductQty;

        await using var transaction = await dbContext.Database.BeginTransactionAsync(stoppingToken);

        try
        {
            var rowsAffected = await dbContext.Products
            .Where(product => product.ProductId == message.ProductId)
            .ExecuteUpdateAsync(product => product
                .SetProperty(p => p.ProductQty, p => message.ProductQty), cancellationToken: stoppingToken);

            if (rowsAffected != 1) throw new TransactionAbortedException();
            
            await transaction.CommitAsync(stoppingToken);

            logger.LogInformation("Product qty updated, commiting offset");
            _consumer.Commit();
        }
        catch (TransactionAbortedException e)
        {
            logger.LogError("Product qty update failed, rolling back transaction");
            await transaction.RollbackAsync(stoppingToken);
        }
    }

    protected override string GetTopic()
    {
        return "product-qty-updated-topic";
    }
}
