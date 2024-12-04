using MediatR;
using vendingmachines.commands.cmds;
using vendingmachines.commands.domain.Entites;
using vendingmachines.commands.eventsourcinghandler;

namespace vendingmachines.commands.application;

public class CreateMachine : IRequestHandler<CreateMachineCommand>
{
    private readonly EventSourcingHandler _eventSourcingHandler;

    public CreateMachine(EventSourcingHandler eventSourcingHandler)
    {
        _eventSourcingHandler = eventSourcingHandler;
    }

    public async Task Handle(CreateMachineCommand request, CancellationToken cancellationToken)
    {
        var machine = new Machine(request);

        await _eventSourcingHandler.Save(machine);
    }
}
