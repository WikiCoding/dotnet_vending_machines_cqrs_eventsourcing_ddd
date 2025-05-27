using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Transactions;
using vendingmachines.queries.contracts;
using vendingmachines.queries.repository;

namespace vendingmachines.queries.consumers;

public class ProductQtyUpdatedMessageHandler(IServiceProvider serviceProvider, ILogger<ProductQtyUpdatedMessageHandler> logger)
{
    public async Task HandleProductQtyUpdatedEvent(string message, IConsumer<string, string> consumer)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var deserialized = JsonSerializer.Deserialize<ProductQtyUpdatedMessage>(message);

        if (deserialized is null)
        {
            logger.LogError("Message wasn't deserialized properly");
            throw new JsonException("Could not deserialize message");
        }

        var product = await dbContext.Products.Where(p => p.ProductId.Equals(deserialized.ProductId)).FirstOrDefaultAsync();

        if (product == null) throw new NullReferenceException("Product not found");

        product.ProductQty = deserialized.ProductQty;

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            var rowsAffected = await dbContext.Products
            .Where(product => product.ProductId == deserialized.ProductId)
            .ExecuteUpdateAsync(product => product
                .SetProperty(p => p.ProductQty, p => deserialized.ProductQty));

            if (rowsAffected != 1) throw new TransactionAbortedException();

            await transaction.CommitAsync();

            logger.LogInformation("Product qty updated, commiting offset");
            consumer.Commit();
        }
        catch (TransactionAbortedException e)
        {
            logger.LogError("Product qty update failed, rolling back transaction");
            await transaction.RollbackAsync();
        }
    }
}
