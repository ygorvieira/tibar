using MediatR;
using Tibar.Application.Common;
using Tibar.Application.DTOs;

namespace Tibar.Application.Queries.Transactions;

public record GetTransactionsByPeriodQuery(
    Guid UserId,
    DateOnly StartDate,
    DateOnly EndDate,
    int Page = 1,
    int PageSize = 50) : IRequest<Result<PagedResult<TransactionDto>>>;
