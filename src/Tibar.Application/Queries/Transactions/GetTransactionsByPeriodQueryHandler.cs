using MediatR;
using Microsoft.EntityFrameworkCore;
using Tibar.Application.Common;
using Tibar.Application.Interfaces;

namespace Tibar.Application.Queries.Transactions;

public class GetTransactionsByPeriodQueryHandler
    : IRequestHandler<GetTransactionsByPeriodQuery, Result<IEnumerable<DTOs.TransactionDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetTransactionsByPeriodQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<DTOs.TransactionDto>>> Handle(
        GetTransactionsByPeriodQuery request, CancellationToken cancellationToken)
    {
        var transactions = await _context.Transactions
            .Where(t => t.UserId == request.UserId
                && t.Date >= request.StartDate
                && t.Date <= request.EndDate)
            .OrderByDescending(t => t.Date)
            .ToListAsync(cancellationToken);

        var categoryIds = transactions.Select(t => t.CategoryId).Distinct();
        var categories = await _context.Categories
            .Where(c => categoryIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        var dtos = transactions.Select(t => new DTOs.TransactionDto(
            t.Id,
            t.Description,
            t.Amount.Amount,
            t.Amount.Currency,
            t.Type.ToString(),
            t.CategoryId,
            categories.GetValueOrDefault(t.CategoryId, ""),
            t.Date,
            t.CreatedAt));

        return Result.Success<IEnumerable<DTOs.TransactionDto>>(dtos);
    }
}
