using MediatR;
using Tibar.Application.Common;
using Tibar.Application.DTOs;

namespace Tibar.Application.Queries.Accounts;

public record GetAccountsQuery(Guid UserId, int Page = 1, int PageSize = 50) : IRequest<Result<PagedResult<AccountDto>>>;
