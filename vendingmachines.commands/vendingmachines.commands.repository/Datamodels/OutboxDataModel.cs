using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using vendingmachines.commands.domain.DomainEvents;

namespace vendingmachines.commands.persistence.Datamodels;

public class OutboxDataModel
{
    [JsonConverter(typeof(ObjectIdJsonConverter))]
    [BsonId]
    public ObjectId OutboxId { get; set; }
    public required BaseDomainEvent BaseDomainEvent { get; set; }
    public bool IsProcessed { get; set; } = false;
}
