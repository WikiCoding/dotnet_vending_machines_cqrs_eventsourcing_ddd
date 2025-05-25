namespace vendingmachines.commands.persistence.Datamodels;

public class ProductSnapshotDataModel
{
    public Guid ProductId { get; set; }
    public required string ProductName { get; set; }
    public int ProductQty { get; set; }
}
