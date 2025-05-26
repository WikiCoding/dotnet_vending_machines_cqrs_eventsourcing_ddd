using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using vendingmachines.commands.application;
using vendingmachines.commands.cmds;
using vendingmachines.commands.contracts;

namespace vendingmachines.commands.controllers.Controllers;

[ApiController]
[Route("[controller]")]
public class MachinesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<MachinesController> _logger;
    /// <summary>
    /// For debugging purposes only
    /// </summary>
    private readonly CheckMachineStatus _checkMachineStatus;

    public MachinesController(IMediator mediator, CheckMachineStatus checkMachineStatus, ILogger<MachinesController> logger)
    {
        _mediator = mediator;
        _checkMachineStatus = checkMachineStatus;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateMachine([FromBody] CreateMachineDto createMachineDto)
    {
        var stopwatch = Stopwatch.StartNew();

        var command = new CreateMachineCommand(createMachineDto.machineType);
        await _mediator.Send(command);

        stopwatch.Stop();
        _logger.LogInformation("Create machine request took {} ms", stopwatch.ElapsedMilliseconds);

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
        var stopwatch = Stopwatch.StartNew();

        var machine = await _checkMachineStatus.Handle(machineId);

        stopwatch.Stop();
        _logger.LogInformation("Query machine request took {} ms", stopwatch.ElapsedMilliseconds);

        return Ok(machine);
    }
}
