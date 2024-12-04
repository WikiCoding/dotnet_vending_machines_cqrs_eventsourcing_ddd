using MediatR;
using Microsoft.AspNetCore.Mvc;
using vendingmachines.commands.application;
using vendingmachines.commands.cmds;
using vendingmachines.commands.contracts;

namespace vendingmachines.commands.controllers.Controllers;

[ApiController]
[Route("[controller]")]
public class MachinesController : ControllerBase
{
    private readonly IMediator _mediator;
    /// <summary>
    /// For debugging purposes only
    /// </summary>
    private readonly CheckMachineStatus _checkMachineStatus;

    public MachinesController(IMediator mediator, CheckMachineStatus checkMachineStatus)
    {
        _mediator = mediator;
        _checkMachineStatus = checkMachineStatus;
    }

    [HttpPost]
    public async Task<IActionResult> CreateMachine([FromBody] CreateMachineDto createMachineDto)
    {
        var command = new CreateMachineCommand(createMachineDto.machineType);
        await _mediator.Send(command);

        return NoContent();
    }
    
    /// <summary>
    /// For debugging purposes only
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    [HttpGet("{machine-id}")]
    public async Task<IActionResult> GetMachineCurrentState([FromRoute(Name = "machine-id")] string machineId)
    {
        var machine = await _checkMachineStatus.Handle(machineId);

        return Ok(machine);
    }
}
