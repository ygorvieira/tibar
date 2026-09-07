using MediatR;
using Microsoft.EntityFrameworkCore;
using Tibar.Application.Common;
using Tibar.Application.Interfaces;

namespace Tibar.Application.Queries.Accounts;

public class GetAccountsQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetAccountsQuery, Result<PagedResult<DTOs.AccountDto>>>
{
    public async Task<Result<PagedResult<DTOs.AccountDto>>> Handle(
        GetAccountsQuery request, CancellationToken cancellationToken)
    {
        var query = context.Accounts
            .Where(a => a.UserId == request.UserId);

        var totalCount = await query.CountAsync(cancellationToken);

        var dtos = await query
            .OrderBy(a => a.Description)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new DTOs.AccountDto(
                a.Id, a.Description, a.Type.ToString(), a.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success(new PagedResult<DTOs.AccountDto>(dtos, totalCount, request.Page, request.PageSize));
    }
}
