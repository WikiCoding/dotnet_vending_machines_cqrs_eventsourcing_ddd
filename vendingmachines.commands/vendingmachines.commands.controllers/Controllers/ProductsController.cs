using MediatR;
using Microsoft.AspNetCore.Mvc;
using vendingmachines.commands.cmds;
using vendingmachines.commands.contracts;

namespace vendingmachines.commands.controllers.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost]
    public async Task<IActionResult> AddProductToMachine([FromBody] AddProductDto addProductDto, 
        [FromQuery(Name="machine-id")] string machineId)
    {
        var command = new AddProductCommand(machineId, addProductDto.productName, addProductDto.productQty);
        await _mediator.Send(command);

        return NoContent();
    }
    
    [HttpPatch("{product-id}")]
    public async Task<IActionResult> UpdateProductStock([FromBody] UpdateProductStockDto updateProductStockDto, 
        [FromRoute(Name = "product-id")] string productId,
        [FromQuery(Name = "machine-id")] string machineId)
    {
        var command = new UpdateProductStockCommand(machineId, productId, updateProductStockDto.qtyToIncrement);
        var result = await _mediator.Send(command);

        return Ok(result);
    }
}