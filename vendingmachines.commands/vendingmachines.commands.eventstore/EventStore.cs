using vendingmachines.commands.domain.DomainEvents;
using vendingmachines.commands.persistence.Datamodels;
using vendingmachines.commands.persistence.Repository;

namespace vendingmachines.commands.eventstore;

public class EventStore
{
    private readonly IEventsRepository _eventsRepository;

    public EventStore(IEventsRepository eventsRepository)
    {
        _eventsRepository = eventsRepository;
    }

    public async Task<List<BaseDomainEvent>> GetEventsByAggregateId(string aggId)
    {
        var events = await _eventsRepository.FindByAggId(aggId);

        if (events.Count == 0) throw new Exception("Events with that aggregate id were not found");
        
        return events.Select(e => e.DomainEvent).ToList();
    }

    public async Task SaveEvents(string aggId, IReadOnlyList<BaseDomainEvent> events, int expectedVersion)
    {
        var eventStream = await _eventsRepository.FindByAggId(aggId);
        if (eventStream.Count > 0 && eventStream.Last().Version >= expectedVersion)
        {
            throw new Exception("Concurrency exception");
        }

        var version = expectedVersion;

        foreach (var evnt in events)
        {
            evnt.Version = version;

            var eventDm = new EventsDataModel
            {
                AggregateId = evnt.AggregateId,
                DomainEvent = evnt,
                CreatedAt = evnt.CreatedAt,
                Version = version,
            };

            version++;

            await _eventsRepository.Save(eventDm);

            // we can produce the message here
        }
    }
}
