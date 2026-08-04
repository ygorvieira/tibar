using MediatR;
using Tibar.Application.Common;
using Tibar.Application.DTOs;

namespace Tibar.Application.Commands.Accounts.Create;

public record CreateAccountCommand(
    string Description,
    string Type,
    Guid UserId) : IRequest<Result<AccountDto>>;
