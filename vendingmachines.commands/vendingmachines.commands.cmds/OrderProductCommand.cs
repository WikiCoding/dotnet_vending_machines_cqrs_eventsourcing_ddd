using MediatR;

namespace vendingmachines.commands.cmds;

public record OrderProductCommand(string aggId, string productId, int orderQty) : IRequest;