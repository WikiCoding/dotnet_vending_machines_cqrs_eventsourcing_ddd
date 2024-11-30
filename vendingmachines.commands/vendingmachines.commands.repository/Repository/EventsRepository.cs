using MongoDB.Driver;
using vendingmachines.commands.contracts;
using vendingmachines.commands.domain.DomainEvents;
using vendingmachines.commands.domain.Entites;
using vendingmachines.commands.domain.ValueObjects;
using vendingmachines.commands.persistence.Datamodels;
using vendingmachines.commands.persistence.MongoDbConfig;

namespace vendingmachines.commands.persistence.Repository;

public class EventsRepository : IEventsRepository
{
    private const string CollectionName = "machines";
    private MongoConfig _mongoConfig;
    private IMongoDatabase _database;

    public EventsRepository(MongoConfig mongoConfig)
    {
        _mongoConfig = mongoConfig;
        _database = _mongoConfig.GetDatabaseConnection();
    }

    public async Task<EventsDataModel> SaveMachine(Machine machine)
    {
        var machineDm = new EventsDataModel
        {
            MachineId = machine.MachineId.Id.ToString(),
            DomainEvent = new MachineCreatedEvent
            {
                EventType = nameof(MachineCreatedEvent),
                MachineType = machine.MachineType.Type,
                CreatedAt = DateTime.UtcNow
            }
        };

        await _database.GetCollection<EventsDataModel>(CollectionName).InsertOneAsync(machineDm);

        return machineDm;
    }

    public async Task<List<EventsDataModel>> FindEventsByMachineId(string machineId)
    {
        var evnts = await _database.GetCollection<EventsDataModel>(CollectionName).Find(el => el.MachineId == machineId).ToListAsync();

        var createEvent = (MachineCreatedEvent) evnts.First().DomainEvent;

        var machine = new Machine(new CreateMachineCommand(createEvent.MachineType));



        return evnts;
    }
}
