namespace vendingmachines.queries.controllers.Dtos;

public class ProductDto
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int ProductQty { get; set; }
    public string MachineId { get; set; } = string.Empty;
}
