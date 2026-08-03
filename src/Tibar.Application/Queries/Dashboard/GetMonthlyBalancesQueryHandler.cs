using MediatR;
using Microsoft.EntityFrameworkCore;
using Tibar.Application.Common;
using Tibar.Application.Interfaces;
using Tibar.Domain.Enums;

namespace Tibar.Application.Queries.Dashboard;

public class GetMonthlyBalancesQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetMonthlyBalancesQuery, Result<List<DTOs.MonthlyBalanceDto>>>
{
    public async Task<Result<List<DTOs.MonthlyBalanceDto>>> Handle(
        GetMonthlyBalancesQuery request, CancellationToken cancellationToken)
    {
        var query = context.Transactions
            .Where(t => t.UserId == request.UserId
                && t.Date >= request.StartDate
                && t.Date <= request.EndDate);

        if (request.CategoryId.HasValue)
            query = query.Where(t => t.CategoryId == request.CategoryId.Value);

        if (request.AccountId.HasValue)
            query = query.Where(t => t.AccountId == request.AccountId.Value);

        if (!string.IsNullOrWhiteSpace(request.Type))
            query = query.Where(t => t.Type.ToString() == request.Type);

        var monthly = await query
            .GroupBy(t => new { t.Date.Year, t.Date.Month })
            .Select(g => new DTOs.MonthlyBalanceDto(
                g.Key.Year,
                g.Key.Month,
                g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount.Amount),
                g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount.Amount),
                0,
                "BRL"))
            .ToListAsync(cancellationToken);

        var result = monthly
            .Select(m => m with { Balance = m.TotalIncome - m.TotalExpense })
            .OrderByDescending(m => m.Year)
            .ThenByDescending(m => m.Month)
            .ToList();

        return Result.Success(result);
    }
}
