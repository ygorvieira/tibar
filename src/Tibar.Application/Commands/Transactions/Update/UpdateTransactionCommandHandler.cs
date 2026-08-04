using MediatR;
using Tibar.Application.Common;
using Tibar.Application.Interfaces;
using Tibar.Domain.Exceptions;
using Tibar.Domain.ValueObjects;

namespace Tibar.Application.Commands.Transactions.Update;

public class UpdateTransactionCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateTransactionCommand, Result<DTOs.TransactionDto>>
{
    public async Task<Result<DTOs.TransactionDto>> Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await context.Transactions
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (transaction is null || transaction.UserId != request.UserId)
            return Result.Failure<DTOs.TransactionDto>("Transação não encontrada.");

        var category = await context.Categories
            .FindAsync(new object[] { request.CategoryId }, cancellationToken);

        if (category is null || category.UserId != request.UserId)
            return Result.Failure<DTOs.TransactionDto>("Categoria não encontrada.");

        var account = await context.Accounts
            .FindAsync(new object[] { request.AccountId }, cancellationToken);

        if (account is null || account.UserId != request.UserId)
            return Result.Failure<DTOs.TransactionDto>("Conta não encontrada.");

        Money amount;
        try
        {
            amount = Money.Create(request.Amount);
        }
        catch (DomainException ex)
        {
            return Result.Failure<DTOs.TransactionDto>(ex.Message);
        }

        transaction.UpdateDescription(request.Description);
        transaction.UpdateAmount(amount);
        transaction.UpdateCategory(request.CategoryId);
        transaction.UpdateAccount(request.AccountId);
        transaction.UpdateDate(request.Date);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new DTOs.TransactionDto(
            transaction.Id,
            transaction.Description,
            transaction.Amount.Amount,
            transaction.Amount.Currency,
            transaction.Type.ToString(),
            transaction.CategoryId,
            category.Name,
            transaction.AccountId,
            account.Description,
            transaction.Date,
            transaction.CreatedAt));
    }
}
