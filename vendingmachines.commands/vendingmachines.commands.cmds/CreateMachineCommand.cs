using MediatR;

namespace vendingmachines.commands.cmds;

public record CreateMachineCommand(string machineType) : IRequest;
