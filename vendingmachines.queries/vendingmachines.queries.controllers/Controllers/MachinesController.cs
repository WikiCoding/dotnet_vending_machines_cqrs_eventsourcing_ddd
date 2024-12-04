using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using vendingmachines.queries.application;
using vendingmachines.queries.controllers.Dtos;

namespace vendingmachines.queries.controllers.Controllers;

[ApiController]
[Route("[controller]")]
public class MachinesController : ControllerBase
{
    private readonly ApplicationService _applicationService;
    private readonly IMapper _mapper;

    public MachinesController(ApplicationService applicationService, IMapper mapper)
    {
        _applicationService = applicationService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> FindAllMachines()
    {
        var machines = await _applicationService.FindAll();

        var machinesDto = _mapper.Map<List<MachineDto>>(machines);

        return Ok(machinesDto);
    }

    [HttpGet("{machine-id}")]
    public async Task<IActionResult> GetMachineById([FromRoute(Name = "machine-id")] string machineId)
    {
        var machine = await _applicationService.FindById(machineId);
        return Ok(machine);
    }
}
