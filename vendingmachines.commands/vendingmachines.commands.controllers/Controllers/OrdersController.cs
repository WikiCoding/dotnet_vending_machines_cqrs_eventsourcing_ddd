using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using vendingmachines.commands.cmds;
using vendingmachines.commands.contracts;

namespace vendingmachines.commands.controllers.Controllers;

[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPut]
    public async Task<IActionResult> OrderProduct([FromBody] OrderProductDto orderProductDto, 
        [FromQuery(Name = "machine-id")] string machineId,
        [FromQuery(Name = "product-id")] string productId)
    {
        var stopwatch = Stopwatch.StartNew();

        var command = new OrderProductCommand(machineId, productId, orderProductDto.orderQty);
        
        await _mediator.Send(command);

        stopwatch.Stop();
        _logger.LogInformation("Order product from machine request took {} ms", stopwatch.ElapsedMilliseconds);

        return NoContent();
    }
}