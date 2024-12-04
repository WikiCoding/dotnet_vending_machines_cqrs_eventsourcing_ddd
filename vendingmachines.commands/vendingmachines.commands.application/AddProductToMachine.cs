using MediatR;
using vendingmachines.commands.cmds;
using vendingmachines.commands.domain.ValueObjects;
using vendingmachines.commands.eventsourcinghandler;

namespace vendingmachines.commands.application;

public class AddProductToMachine : IRequestHandler<AddProductCommand>
{
    private readonly EventSourcingHandler _eventSourcingHandler;
    private readonly SemaphoreSlim _semaphore;

    public AddProductToMachine(EventSourcingHandler eventSourcingHandler)
    {
        _eventSourcingHandler = eventSourcingHandler;
        _semaphore = new SemaphoreSlim(1, 1);
    }

    public async Task Handle(AddProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _semaphore.WaitAsync(cancellationToken);
            
            var machine = await _eventSourcingHandler.GetAggregateById(request.machineId);

            var productId = new ProductId(Guid.NewGuid());
            var productName = new ProductName(request.productName);
            var productQty = new ProductQty(request.productQty);

            machine.AddProduct(productId, productName, productQty);

            await _eventSourcingHandler.Save(machine);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
