using MediatR;
using Microsoft.EntityFrameworkCore;
using Tibar.Application.Common;
using Tibar.Application.Interfaces;
using Tibar.Domain.Enums;

namespace Tibar.Application.Queries.Reports;

public class GetExpenseReportQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetExpenseReportQuery, Result<DTOs.ExpenseReportDto>>
{
    public async Task<Result<DTOs.ExpenseReportDto>> Handle(
        GetExpenseReportQuery request, CancellationToken cancellationToken)
    {
        var expenses = await context.Transactions
            .Where(t => t.UserId == request.UserId
                && t.Type == TransactionType.Expense
                && t.Date >= request.StartDate
                && t.Date <= request.EndDate)
            .Select(t => new
            {
                t.Id,
                t.Description,
                t.Amount.Amount,
                t.Date.Year,
                t.Date.Month,
                t.CategoryId,
                CategoryName = t.Category.Name
            })
            .ToListAsync(cancellationToken);

        var monthly = expenses
            .GroupBy(t => new { t.Year, t.Month })
            .OrderByDescending(g => g.Key.Year)
            .ThenByDescending(g => g.Key.Month)
            .Select(g =>
            {
                var categories = g
                    .GroupBy(t => new { t.CategoryId, t.CategoryName })
                    .Select(cg => new DTOs.CategoryExpenseDto(
                        cg.Key.CategoryId,
                        cg.Key.CategoryName,
                        cg.Sum(t => t.Amount),
                        cg
                            .GroupBy(t => t.Description)
                            .Select(dg => new DTOs.DescriptionSummaryDto(
                                dg.Key,
                                dg.Count(),
                                dg.Sum(t => t.Amount)))
                            .OrderByDescending(d => d.Occurrences)
                            .Take(5)
                            .ToList()))
                    .OrderByDescending(c => c.TotalAmount)
                    .Take(3)
                    .ToList();

                return new DTOs.MonthlyCategoryReportDto(
                    g.Key.Year,
                    g.Key.Month,
                    categories);
            })
            .ToList();

        return Result.Success(new DTOs.ExpenseReportDto(monthly));
    }
}
