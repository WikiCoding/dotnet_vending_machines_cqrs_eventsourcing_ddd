using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Transactions;
using vendingmachines.queries.contracts;
using vendingmachines.queries.repository;

namespace vendingmachines.queries.consumers;

public class ProductOrderedTopicConsumer : BaseConsumer<ProductOrderedMessage>
{
    private readonly ILogger<ProductOrderedTopicConsumer> _logger;
    public ProductOrderedTopicConsumer(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<ProductOrderedTopicConsumer> logger) : base(configuration, serviceProvider, logger)
    {
        _logger = logger;
    }

    protected override string GetTopic()
    {
        return "product-ordered-topic";
    }

    protected override async Task HandleMessageAsync(ProductOrderedMessage message, IServiceProvider serviceProvider, CancellationToken stoppingToken, IConsumer<string, string> _consumer)
    {
        var dbContext = serviceProvider.GetRequiredService<AppDbContext>();

        var product = await dbContext.Products.Where(p => p.ProductId.Equals(message.ProductId)).FirstOrDefaultAsync(stoppingToken);

        if (product == null) throw new NullReferenceException("Product not found");

        product.ProductQty -= message.OrderedQty;

        await using var transaction = await dbContext.Database.BeginTransactionAsync(stoppingToken);

        try
        {
            var rowsAffected = await dbContext.Products
            .Where(product => product.ProductId == message.ProductId)
            .ExecuteUpdateAsync(product => product
                .SetProperty(p => p.ProductQty, p => p.ProductQty - message.OrderedQty), stoppingToken);

            if (rowsAffected != 1) throw new TransactionAbortedException("Product qty update failed");

            await transaction.CommitAsync(stoppingToken);

            _logger.LogInformation("Updated qty of product after order successfully. Commiting offsets");
            _consumer.Commit();
        }
        catch (TransactionAbortedException e)
        {
            _logger.LogError("Product qty update failed, rolling back transaction");

        }
    }
}
