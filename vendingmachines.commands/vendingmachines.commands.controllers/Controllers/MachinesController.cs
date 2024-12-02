using MediatR;
using Microsoft.AspNetCore.Mvc;
using vendingmachines.commands.app;
using vendingmachines.commands.contracts;

namespace vendingmachines.commands.controllers.Controllers;

[ApiController]
[Route("[controller]")]
public class MachinesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly CheckMachineStatus _checkMachineStatus;

    public MachinesController(IMediator mediator, CheckMachineStatus checkMachineStatus)
    {
        _mediator = mediator;
        _checkMachineStatus = checkMachineStatus;
    }

    [HttpGet("{machine-id}")]
    public async Task<IActionResult> GetMachineCurrentState([FromRoute(Name = "machine-id")] string machineId)
    {
        var machine = await _checkMachineStatus.Handle(machineId);

        return Ok(machine);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMachine([FromBody] CreateMachineCommand createMachineCommand)
    {
        await _mediator.Send(createMachineCommand);

        return Created();
    }

    [HttpPost("{machine-id}")]
    public async Task<IActionResult> AddProductToMachine([FromBody] AddProductCommand command, [FromRoute(Name="machine-id")] string machineId)
    {
        await _mediator.Send(command);

        return Created();
    }

    [HttpPatch("{machine-id}/{product-id}")]
    public async Task<IActionResult> UpdateProductStock([FromBody] UpdateProductStockCommand command, 
                                                        [FromRoute(Name = "machine-id")] string machineId, 
                                                        [FromRoute(Name = "product-id")] string productId)
    {
        var result = await _mediator.Send(command);

        return Ok(result);
    }
}
