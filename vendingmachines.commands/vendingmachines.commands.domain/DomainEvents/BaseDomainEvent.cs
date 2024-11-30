using MongoDB.Bson.Serialization.Attributes;

namespace vendingmachines.commands.domain.DomainEvents;

[BsonKnownTypes(typeof(MachineCreatedEvent))]
public abstract class BaseDomainEvent
{
}
