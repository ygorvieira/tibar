using MediatR;
using Microsoft.EntityFrameworkCore;
using Tibar.Application.Common;
using Tibar.Application.Interfaces;

namespace Tibar.Application.Queries.Accounts;

public class GetAccountsQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetAccountsQuery, Result<IEnumerable<DTOs.AccountDto>>>
{
    public async Task<Result<IEnumerable<DTOs.AccountDto>>> Handle(
        GetAccountsQuery request, CancellationToken cancellationToken)
    {
        var dtos = await context.Accounts
            .Where(a => a.UserId == request.UserId)
            .OrderBy(a => a.Description)
            .Select(a => new DTOs.AccountDto(
                a.Id, a.Description, a.Type.ToString(), a.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<DTOs.AccountDto>>(dtos);
    }
}
