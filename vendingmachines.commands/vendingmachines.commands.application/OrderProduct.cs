using MediatR;
using vendingmachines.commands.cmds;
using vendingmachines.commands.domain.ValueObjects;
using vendingmachines.commands.eventsourcinghandler;

namespace vendingmachines.commands.application;

public class OrderProduct : IRequestHandler<OrderProductCommand>
{
    private readonly EventSourcingHandler _eventSourcingHandler;

    public OrderProduct(EventSourcingHandler eventSourcingHandler)
    {
        _eventSourcingHandler = eventSourcingHandler;
    }

    public async Task Handle(OrderProductCommand request, CancellationToken cancellationToken)
    {
        var machine = await _eventSourcingHandler.GetAggregateById(request.aggId);
        var productId = new ProductId(Guid.Parse(request.productId));
        var productQty = new ProductQty(request.orderQty);
        
        machine.OrderProduct(productId, productQty);

        await _eventSourcingHandler.Save(machine);
    }
}