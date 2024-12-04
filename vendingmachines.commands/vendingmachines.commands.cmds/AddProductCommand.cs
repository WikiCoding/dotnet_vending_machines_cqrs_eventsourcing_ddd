using MediatR;

namespace vendingmachines.commands.cmds;

public record AddProductCommand(string machineId, string productName, int productQty) : IRequest;

