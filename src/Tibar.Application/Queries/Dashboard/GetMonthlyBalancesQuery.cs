using MediatR;
using Tibar.Application.Common;
using Tibar.Application.DTOs;

namespace Tibar.Application.Queries.Dashboard;

public record GetMonthlyBalancesQuery(
    Guid UserId,
    DateOnly StartDate,
    DateOnly EndDate,
    Guid? CategoryId = null,
    Guid? AccountId = null,
    string? Type = null) : IRequest<Result<List<MonthlyBalanceDto>>>;
