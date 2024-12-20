﻿using MediatR;
using vendingmachines.commands.cmds;
using vendingmachines.commands.contracts;
using vendingmachines.commands.eventsourcinghandler;

namespace vendingmachines.commands.application;

public class UpdateProductStock : IRequestHandler<UpdateProductStockCommand, MachineDto>
{
    private readonly EventSourcingHandler _eventSourcingHandler;
    private readonly SemaphoreSlim _semaphore;

    public UpdateProductStock(EventSourcingHandler eventSourcingHandler)
    {
        _semaphore = new SemaphoreSlim(1, 1);
        _eventSourcingHandler = eventSourcingHandler;
    }

    public async Task<MachineDto> Handle(UpdateProductStockCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _semaphore.WaitAsync(cancellationToken);
            
            var machine = await _eventSourcingHandler.GetAggregateById(request.aggId);
            machine.UpdateProductStock(request.productId, request.qtyToIncrement);
            await _eventSourcingHandler.Save(machine);

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
