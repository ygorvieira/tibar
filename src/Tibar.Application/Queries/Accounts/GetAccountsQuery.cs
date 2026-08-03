using MediatR;
using Tibar.Application.Common;
using Tibar.Application.DTOs;

namespace Tibar.Application.Queries.Accounts;

public record GetAccountsQuery(Guid UserId) : IRequest<Result<IEnumerable<AccountDto>>>;
