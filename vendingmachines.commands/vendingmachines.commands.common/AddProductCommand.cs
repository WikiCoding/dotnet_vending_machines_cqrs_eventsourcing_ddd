using MediatR;

namespace vendingmachines.commands.contracts;

public record AddProductCommand(string machineId, string productName, int productQty) : IRequest;

