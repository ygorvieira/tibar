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
            .Join(_context.Categories,
                t => t.CategoryId,
                c => c.Id,
                (t, c) => new DTOs.TransactionDto(
                    t.Id,
                    t.Description,
                    t.Amount.Amount,
                    t.Amount.Currency,
                    t.Type.ToString(),
                    t.CategoryId,
                    c.Name,
                    t.Date,
                    t.CreatedAt))
            .OrderByDescending(t => t.Date)
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<DTOs.TransactionDto>>(transactions);
    }
}
