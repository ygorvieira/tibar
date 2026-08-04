using MediatR;
using Tibar.Application.Common;
using Tibar.Application.DTOs;

namespace Tibar.Application.Commands.Accounts.Update;

public record UpdateAccountCommand(
    Guid Id,
    string Description,
    string Type,
    Guid UserId) : IRequest<Result<AccountDto>>;
