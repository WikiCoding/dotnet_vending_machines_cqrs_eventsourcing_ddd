using vendingmachines.commands.persistence.Datamodels;

namespace vendingmachines.commands.persistence.Repository;

public interface ISnapshotRepository
{
    Task<SnapshotDataModel?> FindByAggId(string aggId);

    Task SaveSnapshot(SnapshotDataModel snapshotDataModel);
}
