using MediatR;
using vendingmachines.commands.cmds;
using vendingmachines.commands.domain.ValueObjects;
using vendingmachines.commands.eventsourcinghandler;

namespace vendingmachines.commands.application;

public class AddProductToMachine : IRequestHandler<AddProductCommand>
{
    private readonly EventSourcingHandler _eventSourcingHandler;

    public AddProductToMachine(EventSourcingHandler eventSourcingHandler)
    {
        _eventSourcingHandler = eventSourcingHandler;
    }

    public async Task Handle(AddProductCommand request, CancellationToken cancellationToken)
    {
        var machine = await _eventSourcingHandler.GetAggregateById(request.machineId);

        var productId = new ProductId(Guid.NewGuid());
        var productName = new ProductName(request.productName);
        var productQty = new ProductQty(request.productQty);

        machine.AddProduct(productId, productName, productQty);

        await _eventSourcingHandler.Save(machine);
    }
}
