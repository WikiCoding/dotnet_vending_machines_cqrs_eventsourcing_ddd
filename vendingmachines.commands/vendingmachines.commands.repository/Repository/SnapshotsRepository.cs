using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using vendingmachines.commands.persistence.Datamodels;

namespace vendingmachines.commands.persistence.Repository;

public class SnapshotsRepository(ILogger<SnapshotsRepository> logger, IDistributedCache redis)
{
    public async Task<SnapshotDataModel?> FindByAggId(string aggId)
    {
        var aggregateSnapshot = await redis.GetStringAsync(aggId);

        if (string.IsNullOrEmpty(aggregateSnapshot))
        {
            logger.LogInformation("Snapshot not found for key {}", aggId);
            return null;
        }

        return JsonConvert.DeserializeObject<SnapshotDataModel>(aggregateSnapshot);
    }

    public async Task SaveSnapshot(SnapshotDataModel snapshotDataModel)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(355)
        };

        var dataToStore = JsonConvert.SerializeObject(snapshotDataModel);

        logger.LogInformation("Snapshoting key {} and value {}", snapshotDataModel.AggregateId, dataToStore);

        await redis.SetStringAsync(snapshotDataModel.AggregateId.ToString(), dataToStore, options);
    }
}
