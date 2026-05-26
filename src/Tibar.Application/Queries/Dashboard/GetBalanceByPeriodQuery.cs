using MediatR;
using Tibar.Application.Common;
using Tibar.Application.DTOs;

namespace Tibar.Application.Queries.Dashboard;

public record GetBalanceByPeriodQuery(
    Guid UserId,
    DateOnly StartDate,
    DateOnly EndDate) : IRequest<Result<BalanceDto>>;
