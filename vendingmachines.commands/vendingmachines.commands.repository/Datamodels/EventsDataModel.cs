using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using vendingmachines.commands.domain.DomainEvents;

namespace vendingmachines.commands.persistence.Datamodels;

public class EventsDataModel
{
    [BsonId]
    public ObjectId Id { get; set; }
    [BsonElement("machine_id")]
    public string AggregateId { get; set; } = string.Empty;

    [BsonElement("event")]
    public BaseDomainEvent DomainEvent { get; set; }
    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [BsonElement("version")]
    public int Version { get; set; }
}
