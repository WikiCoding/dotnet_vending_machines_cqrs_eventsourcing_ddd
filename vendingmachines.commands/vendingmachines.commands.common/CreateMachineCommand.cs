using MediatR;

namespace vendingmachines.commands.contracts;

public record CreateMachineCommand(string machineType) : IRequest;
