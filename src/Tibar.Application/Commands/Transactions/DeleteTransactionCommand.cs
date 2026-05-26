using MediatR;
using Tibar.Application.Common;

namespace Tibar.Application.Commands.Transactions;

public record DeleteTransactionCommand(Guid Id, Guid UserId) : IRequest<Result<Unit>>;
