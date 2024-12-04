using MediatR;

namespace vendingmachines.commands.cmds;

public record RebuildQueriesDbCommand(): IRequest;