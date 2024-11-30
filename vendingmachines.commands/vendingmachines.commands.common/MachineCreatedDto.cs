using MediatR;

namespace vendingmachines.commands.contracts;

public record MachineCreatedDto(string id, string eventId, string eventType, string machineType, DateTime createdAt) : IRequest<MachineCreatedDto>;
