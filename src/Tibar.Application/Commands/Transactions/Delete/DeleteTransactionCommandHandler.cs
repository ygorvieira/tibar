using MediatR;
using Tibar.Application.Common;
using Tibar.Application.Interfaces;

namespace Tibar.Application.Commands.Transactions.Delete;

public class DeleteTransactionCommandHandler(
    IApplicationDbContext context) : IRequestHandler<DeleteTransactionCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await context.Transactions
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (transaction is null || transaction.UserId != request.UserId)
            return Result.Failure<Unit>("Transação não encontrada.");

        context.Transactions.Remove(transaction);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(Unit.Value);
    }
}
