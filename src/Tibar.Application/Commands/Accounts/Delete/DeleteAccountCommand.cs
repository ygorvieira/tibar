using MediatR;
using Tibar.Application.Common;

namespace Tibar.Application.Commands.Accounts.Delete;

public record DeleteAccountCommand(Guid Id, Guid UserId) : IRequest<Result<Unit>>;
