using MediatR;
using vendingmachines.commands.cmds;
using vendingmachines.commands.eventsourcinghandler;

namespace vendingmachines.commands.application;

public class RebuildQueriesDb : IRequestHandler<RebuildQueriesDbCommand>
{
    private readonly EventSourcingHandler _eventSourcingHandler;

    public RebuildQueriesDb(EventSourcingHandler eventSourcingHandler)
    {
        _eventSourcingHandler = eventSourcingHandler;
    }

    public async Task Handle(RebuildQueriesDbCommand request, CancellationToken cancellationToken)
    {
        await _eventSourcingHandler.RebuildQueriesDbState();
    }
}