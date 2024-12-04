using MediatR;
using Microsoft.AspNetCore.Mvc;
using vendingmachines.commands.cmds;
using vendingmachines.commands.contracts;

namespace vendingmachines.commands.controllers.Controllers;

[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut]
    public async Task<IActionResult> OrderProduct([FromBody] OrderProductDto orderProductDto, 
        [FromQuery(Name = "machine-id")] string machineId,
        [FromQuery(Name = "product-id")] string productId)
    {
        var command = new OrderProductCommand(machineId, productId, orderProductDto.orderQty);
        
        await _mediator.Send(command);

        return NoContent();
    }
}