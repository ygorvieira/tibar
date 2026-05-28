using MediatR;
using Tibar.Application.Common;

namespace Tibar.Application.Commands.Transactions.Delete;

public record DeleteTransactionCommand(Guid Id, Guid UserId) : IRequest<Result<Unit>>;
