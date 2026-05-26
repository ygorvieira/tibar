using MediatR;
using Tibar.Application.Common;
using Tibar.Application.DTOs;

namespace Tibar.Application.Queries.Transactions;

public record GetTransactionsByPeriodQuery(
    Guid UserId,
    DateOnly StartDate,
    DateOnly EndDate) : IRequest<Result<IEnumerable<TransactionDto>>>;
