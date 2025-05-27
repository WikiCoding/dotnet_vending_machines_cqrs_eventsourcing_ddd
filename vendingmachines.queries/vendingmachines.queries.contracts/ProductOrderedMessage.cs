namespace vendingmachines.queries.contracts;

public class ProductOrderedMessage : BaseEventMessage
{
    public string OrderId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public int OrderedQty { get; set; }
    public string EventType { get; init; } = string.Empty;
}
