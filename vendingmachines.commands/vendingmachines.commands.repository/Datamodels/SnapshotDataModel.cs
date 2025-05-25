namespace vendingmachines.commands.persistence.Datamodels;

public class SnapshotDataModel
{
    public Guid AggregateId { get; set; }
    public required string MachineType { get; set; }
    public int Version { get; set; }
    public List<ProductSnapshotDataModel> products { get; set; }
    public DateTime SnapshotCreated { get; set; } = DateTime.UtcNow;
}
