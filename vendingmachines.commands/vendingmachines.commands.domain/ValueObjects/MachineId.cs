using vendingmachines.commands.domain.DDD;

namespace vendingmachines.commands.domain.ValueObjects;

public class MachineId : IEntityId, IValueObject
{
    public Guid Id { get; }

    public MachineId(Guid id)
    {
        if (id == Guid.Empty) { throw new ArgumentNullException("invalid id"); }
        Id = id;
    }
}
