namespace vendingmachines.queries.contracts;

public abstract class BaseEventMessage
{
    public int Version { get; set; } = 0;
    public string AggregateId { get; set; }
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}
