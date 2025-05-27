using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Transactions;
using vendingmachines.queries.contracts;
using vendingmachines.queries.repository;

namespace vendingmachines.queries.consumers;

public class ProductOrderedMessageHandler(IServiceProvider serviceProvider, ILogger<ProductOrderedMessageHandler> logger)
{
    public async Task HandleProductOrdered(string message, IConsumer<string, string> consumer)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var deserialized = JsonSerializer.Deserialize<ProductOrderedMessage>(message);

        if (deserialized is null)
        {
            logger.LogError("Message wasn't deserialized properly");
            throw new JsonException("Could not deserialize message");
        }

        var product = await dbContext.Products.Where(p => p.ProductId.Equals(deserialized.ProductId)).FirstOrDefaultAsync();

        if (product == null) throw new NullReferenceException("Product not found");

        product.ProductQty -= deserialized.OrderedQty;

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            var rowsAffected = await dbContext.Products
            .Where(product => product.ProductId == deserialized.ProductId)
            .ExecuteUpdateAsync(product => product
                .SetProperty(p => p.ProductQty, p => p.ProductQty - deserialized.OrderedQty));

            if (rowsAffected != 1) throw new TransactionAbortedException("Product qty update failed");

            await transaction.CommitAsync();

            logger.LogInformation("Updated qty of product after order successfully. Commiting offsets");
            consumer.Commit();
        }
        catch (TransactionAbortedException e)
        {
            logger.LogError("Product qty update failed, rolling back transaction");
        }
    }
}
