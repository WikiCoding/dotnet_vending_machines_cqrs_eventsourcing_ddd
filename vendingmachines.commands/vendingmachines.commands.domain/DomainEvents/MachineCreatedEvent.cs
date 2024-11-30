namespace vendingmachines.commands.domain.DomainEvents;

public class MachineCreatedEvent : BaseDomainEvent
{
    public string EventId { get; init; } = Guid.NewGuid().ToString();
    public string EventType { get; init; } = string.Empty;
    public string MachineType { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
