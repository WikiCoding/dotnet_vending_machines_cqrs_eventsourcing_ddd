using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using vendingmachines.commands.persistence.Datamodels;

namespace vendingmachines.commands.persistence.Repository;

public class SnapshotsRepository(ILogger<SnapshotsRepository> logger, IDistributedCache redis)
{
    public async Task<List<EventsDataModel>> FindByAggId(string aggId)
    {
        var aggregateSnapshot = await redis.GetStringAsync(aggId);

        if (string.IsNullOrEmpty(aggregateSnapshot))
        {
            logger.LogInformation("Snapshot not found for key {}", aggId);
            return [];
        }

        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        var eventDm = JsonConvert.DeserializeObject<EventsDataModel>(aggregateSnapshot, settings);

        return eventDm is not null ? [eventDm] : [];
    }

    public async Task<EventsDataModel> Save(EventsDataModel eventDm)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(355)
        };

        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        var dataToStore = JsonConvert.SerializeObject(eventDm, settings);

        logger.LogInformation("Snapshoting key {} and value {}", eventDm.AggregateId, dataToStore);

        await redis.SetStringAsync(eventDm.AggregateId, dataToStore, options);

        return eventDm;
    }
}
