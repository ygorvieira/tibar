using MediatR;
using Tibar.Application.Common;
using Tibar.Application.Interfaces;

namespace Tibar.Application.Commands.Transactions;

public class DeleteTransactionCommandHandler : IRequestHandler<DeleteTransactionCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public DeleteTransactionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _context.Transactions
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (transaction is null || transaction.UserId != request.UserId)
            return Result.Failure<Unit>("Transaction not found.");

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(Unit.Value);
    }
}
