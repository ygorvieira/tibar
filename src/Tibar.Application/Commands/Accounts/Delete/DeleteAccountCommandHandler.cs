using MediatR;
using Microsoft.EntityFrameworkCore;
using Tibar.Application.Common;
using Tibar.Application.Interfaces;

namespace Tibar.Application.Commands.Accounts.Delete;

public class DeleteAccountCommandHandler(
    IApplicationDbContext context) : IRequestHandler<DeleteAccountCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await context.Accounts
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (account is null || account.UserId != request.UserId)
            return Result.Failure<Unit>("Conta não encontrada.");

        var hasTransactions = await context.Transactions
            .AnyAsync(t => t.AccountId == request.Id, cancellationToken);

        if (hasTransactions)
            return Result.Failure<Unit>("Conta possui transações vinculadas e não pode ser excluída.");

        context.Accounts.Remove(account);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(Unit.Value);
    }
}
