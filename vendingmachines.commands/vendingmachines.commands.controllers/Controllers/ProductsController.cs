using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using vendingmachines.commands.cmds;
using vendingmachines.commands.contracts;

namespace vendingmachines.commands.controllers.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IMediator mediator, ILogger<ProductsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> AddProductToMachine([FromBody] AddProductDto addProductDto, 
        [FromQuery(Name="machine-id")] string machineId)
    {
        var stopwatch = Stopwatch.StartNew();

        var command = new AddProductCommand(machineId, addProductDto.productName, addProductDto.productQty);
        await _mediator.Send(command);

        stopwatch.Stop();
        _logger.LogInformation("Add product to machine request took {} ms", stopwatch.ElapsedMilliseconds);

        return NoContent();
    }
    
    [HttpPatch("{product-id}")]
    public async Task<IActionResult> UpdateProductStock([FromBody] UpdateProductStockDto updateProductStockDto, 
        [FromRoute(Name = "product-id")] string productId,
        [FromQuery(Name = "machine-id")] string machineId)
    {
        var stopwatch = Stopwatch.StartNew();

        var command = new UpdateProductStockCommand(machineId, productId, updateProductStockDto.qtyToIncrement);
        var result = await _mediator.Send(command);

        stopwatch.Stop();
        _logger.LogInformation("Update product stock request took {} ms", stopwatch.ElapsedMilliseconds);

        return Ok(result);
    }
}