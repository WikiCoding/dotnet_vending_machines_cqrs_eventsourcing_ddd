namespace vendingmachines.commands.domain.DomainEvents;

public class MachineCreatedEvent : BaseDomainEvent
{
    public string EventType { get; init; } = string.Empty;
    public string MachineType { get; init; } = string.Empty;
}
