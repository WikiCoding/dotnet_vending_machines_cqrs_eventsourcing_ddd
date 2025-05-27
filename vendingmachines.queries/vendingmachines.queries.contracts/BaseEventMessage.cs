namespace vendingmachines.queries.contracts;

public abstract class BaseEventMessage
{
    public int Version { get; set; } = 0;
    public required string AggregateId { get; set; }
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}
