using MediatR;
using Microsoft.AspNetCore.Mvc;
using vendingmachines.commands.cmds;

namespace vendingmachines.commands.controllers.Controllers;

[ApiController]
[Route("[controller]")]
public class RebuildQueriesDbController : ControllerBase
{
    private readonly IMediator _mediator;

    public RebuildQueriesDbController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> RebuildQueriesDb()
    {
        var command = new RebuildQueriesDbCommand();

        await _mediator.Send(command);

        return Ok();
    }
}