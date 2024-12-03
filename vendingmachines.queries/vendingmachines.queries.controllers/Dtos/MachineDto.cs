using vendingmachines.queries.entities;

namespace vendingmachines.queries.controllers.Dtos;

public class MachineDto
{
    public string MachineId { get; set; } = string.Empty;
    public string MachineType { get; set; } = string.Empty;
    public List<ProductDto> Products { get; set; } = [];
}