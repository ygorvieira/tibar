using MediatR;
using Microsoft.EntityFrameworkCore;
using Tibar.Application.Common;
using Tibar.Application.Interfaces;
using Tibar.Domain.Enums;

namespace Tibar.Application.Queries.Dashboard;

public class GetBalanceByPeriodQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetBalanceByPeriodQuery, Result<DTOs.BalanceDto>>
{
    public async Task<Result<DTOs.BalanceDto>> Handle(
        GetBalanceByPeriodQuery request, CancellationToken cancellationToken)
    {
        var transactions = await context.Transactions
            .Where(t => t.UserId == request.UserId
                && t.Date >= request.StartDate
                && t.Date <= request.EndDate)
            .ToListAsync(cancellationToken);

        var totalIncome = transactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount.Amount);

        var totalExpense = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount.Amount);

        var currency = "BRL";

        return Result.Success(new DTOs.BalanceDto(
            totalIncome,
            totalExpense,
            totalIncome - totalExpense,
            currency,
            request.StartDate,
            request.EndDate));
    }
}
