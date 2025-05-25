using MongoDB.Bson;
using Newtonsoft.Json;

namespace vendingmachines.commands.persistence.Datamodels;

public class ObjectIdJsonConverter : JsonConverter<ObjectId>
{
    public override void WriteJson(JsonWriter writer, ObjectId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }

    public override ObjectId ReadJson(JsonReader reader, Type objectType, ObjectId existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var objectIdString = reader.Value?.ToString();
        return ObjectId.Parse(objectIdString ?? ObjectId.Empty.ToString());
    }
}