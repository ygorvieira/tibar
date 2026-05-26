using MediatR;
using Tibar.Application.Common;
using Tibar.Application.DTOs;

namespace Tibar.Application.Commands.Transactions;

public record CreateTransactionCommand(
    string Description,
    decimal Amount,
    string Type,
    Guid CategoryId,
    Guid UserId,
    DateOnly Date) : IRequest<Result<TransactionDto>>;
