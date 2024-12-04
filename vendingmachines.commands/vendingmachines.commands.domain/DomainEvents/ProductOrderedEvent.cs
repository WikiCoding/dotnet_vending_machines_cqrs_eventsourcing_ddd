namespace vendingmachines.commands.domain.DomainEvents;

public class ProductOrderedEvent : BaseDomainEvent
{
    public string OrderId { get; set; }
    public string ProductId { get; set; }
    public int OrderedQty { get; set; }
}