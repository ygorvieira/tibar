using MediatR;
using Tibar.Application.Common;
using Tibar.Application.DTOs;

namespace Tibar.Application.Queries.Reports;

public record GetExpenseReportQuery(
    Guid UserId,
    DateOnly StartDate,
    DateOnly EndDate,
    Guid? AccountId = null) : IRequest<Result<ExpenseReportDto>>;
