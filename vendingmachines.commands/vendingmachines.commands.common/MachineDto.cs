using System.Reflection.PortableExecutable;

namespace vendingmachines.commands.contracts;

public class MachineDto
{
    public List<ProductDto> Products { get; set; } = [];
    public string MachineId { get; init; } = string.Empty;
    public string MachineType { get; init; } = string.Empty;
    public int Version { get; init; }
}
