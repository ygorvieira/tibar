using MediatR;
using Tibar.Application.Common;
using Tibar.Application.DTOs;

namespace Tibar.Application.Commands.Transactions.Update;

public record UpdateTransactionCommand(
    Guid Id,
    string Description,
    decimal Amount,
    Guid CategoryId,
    Guid UserId,
    DateOnly Date) : IRequest<Result<TransactionDto>>;
