using MediatR;
using Microsoft.AspNetCore.Mvc;
using vendingmachines.commands.contracts;

namespace vendingmachines.commands.controllers.Controllers;

[ApiController]
[Route("[controller]")]
public class MachinesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MachinesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateMachine([FromBody] CreateMachineCommand createMachineCommand)
    {
        var evnt = await _mediator.Send(createMachineCommand);

        return CreatedAtAction(nameof(CreateMachine), evnt);
    }
}
