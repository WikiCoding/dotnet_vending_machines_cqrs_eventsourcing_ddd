namespace vendingmachines.commands.domain.DomainEvents;

public class ProductQtyUpdatedEvent : BaseDomainEvent
{
    public string ProductId { get; init; } = string.Empty;
    public int ProductQty { get; init; }
    public string EventType { get; init; } = string.Empty;
}
