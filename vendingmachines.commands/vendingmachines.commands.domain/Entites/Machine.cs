using vendingmachines.commands.contracts;
using vendingmachines.commands.domain.DDD;
using vendingmachines.commands.domain.DomainEvents;
using vendingmachines.commands.domain.ValueObjects;

namespace vendingmachines.commands.domain.Entites;

public class Machine : IAggregateRoot
{
    private List<BaseDomainEvent> _events = [];
    private List<Product> products = [];
    public MachineId MachineId { get; set; }
    public MachineType MachineType { get; set; }
    public int Version { get; set; } = 0;

    public Machine(CreateMachineCommand command)
    {
        RaiseMachineCreatedEvent(new MachineCreatedEvent { EventType = nameof(CreateMachineCommand), MachineType = command.machineType });
    }

    public void RaiseMachineCreatedEvent(MachineCreatedEvent machineCreatedEvent)
    {
        MachineId = new MachineId(Guid.NewGuid());
        MachineType = new MachineType(machineCreatedEvent.MachineType);
        Version = 1;

        _events.Add(machineCreatedEvent);
    }

    public void RebuildState(List<BaseDomainEvent> events)
    {

    }

    public void EventsCommited()
    {
        _events.Clear();
    }

    public IReadOnlyList<BaseDomainEvent> GetEvents()
    {
        return _events.AsReadOnly();
    }

    public IReadOnlyList<Product> Products => products.AsReadOnly();
}
