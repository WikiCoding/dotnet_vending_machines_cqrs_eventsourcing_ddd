namespace vendingmachines.queries.contracts;

public class MachineCreatedMessage : BaseEventMessage
{
    public string EventType { get; init; } = string.Empty;
    public string MachineType { get; init; } = string.Empty;
}
