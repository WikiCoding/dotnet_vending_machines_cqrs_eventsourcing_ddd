using MediatR;
using vendingmachines.commands.contracts;

namespace vendingmachines.commands.cmds;

public record UpdateProductStockCommand(string aggId, string productId, int qtyToIncrement) : IRequest<MachineDto>;
