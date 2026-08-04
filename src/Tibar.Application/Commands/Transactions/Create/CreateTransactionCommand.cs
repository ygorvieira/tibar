using MediatR;
using Tibar.Application.Common;
using Tibar.Application.DTOs;

namespace Tibar.Application.Commands.Transactions.Create;

public record CreateTransactionCommand(
    string Description,
    decimal Amount,
    string Type,
    Guid CategoryId,
    Guid AccountId,
    Guid UserId,
    DateOnly Date) : IRequest<Result<TransactionDto>>;
