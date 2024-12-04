using MongoDB.Bson.Serialization.Attributes;

namespace vendingmachines.commands.domain.DomainEvents;

[BsonKnownTypes(typeof(MachineCreatedEvent), typeof(ProductAddedEvent), typeof(ProductQtyUpdatedEvent), typeof(ProductOrderedEvent))]
public abstract class BaseDomainEvent
{
    public int Version { get; set; }  = 0;
    public string AggregateId { get; set; }
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}
