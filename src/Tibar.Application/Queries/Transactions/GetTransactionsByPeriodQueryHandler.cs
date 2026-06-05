using MediatR;
using Microsoft.EntityFrameworkCore;
using Tibar.Application.Common;
using Tibar.Application.Interfaces;

namespace Tibar.Application.Queries.Transactions;

public class GetTransactionsByPeriodQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetTransactionsByPeriodQuery, Result<PagedResult<DTOs.TransactionDto>>>
{
    public async Task<Result<PagedResult<DTOs.TransactionDto>>> Handle(
        GetTransactionsByPeriodQuery request, CancellationToken cancellationToken)
    {
        var query = context.Transactions
            .Where(t => t.UserId == request.UserId
                && t.Date >= request.StartDate
                && t.Date <= request.EndDate);

        if (request.CategoryId.HasValue)
            query = query.Where(t => t.CategoryId == request.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(request.Type))
            query = query.Where(t => t.Type.ToString() == request.Type);

        var totalCount = await query.CountAsync(cancellationToken);

        var dtos = await query
            .OrderByDescending(t => t.Date)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new DTOs.TransactionDto(
                t.Id,
                t.Description,
                t.Amount.Amount,
                t.Amount.Currency,
                t.Type.ToString(),
                t.CategoryId,
                t.Category.Name,
                t.Date,
                t.CreatedAt))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<DTOs.TransactionDto>(dtos, totalCount, request.Page, request.PageSize);

        return Result.Success(result);
    }
}
