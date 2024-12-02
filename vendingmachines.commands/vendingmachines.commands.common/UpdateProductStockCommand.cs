using MediatR;

namespace vendingmachines.commands.contracts;

public record UpdateProductStockCommand(string aggId, string productId, int qtyToIcrement) : IRequest<MachineDto>;
