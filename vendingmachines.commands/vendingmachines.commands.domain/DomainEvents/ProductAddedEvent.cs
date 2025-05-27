namespace vendingmachines.commands.domain.DomainEvents;

public class ProductAddedEvent : BaseDomainEvent
{
    public string ProductId { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public int ProductQty { get; init; }
    public string EventType { get; init; } = string.Empty;
}
