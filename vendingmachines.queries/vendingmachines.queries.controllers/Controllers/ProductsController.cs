using Microsoft.AspNetCore.Mvc;
using vendingmachines.queries.application;

namespace vendingmachines.queries.controllers.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ApplicationService _applicationService;

    public ProductsController(ApplicationService applicationService)
    {
        _applicationService = applicationService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetMachineProducts([FromQuery(Name = "machine-id")] string machineId)
    {
        var products = await _applicationService.FindProductsByMachineId(machineId);
        return Ok(products);
    }
}