using MediatR;
using vendingmachines.commands.contracts;
using vendingmachines.commands.eventsourcinghandler;

namespace vendingmachines.commands.app;

public class CheckMachineStatus
{
    private readonly EventSourcingHandler _eventSourcingHandler;

    public CheckMachineStatus(EventSourcingHandler eventSourcingHandler)
    {
        _eventSourcingHandler = eventSourcingHandler;
    }

    public async Task<MachineDto> Handle(string aggId)
    {
        var machine = await _eventSourcingHandler.GetAggregateById(aggId);

        var machineProductsDto = machine.GetProducts().ToList().ConvertAll(product => 
            new ProductDto(product.ProductId.Id.ToString(), product.ProductName.Name, product.ProductQty.qty));

        return new MachineDto
        {
            Products = machineProductsDto,
            MachineId = machine.MachineId.Id.ToString(),
            MachineType = machine.MachineType.Type,
            Version = machine.Version
        };
    }
}
