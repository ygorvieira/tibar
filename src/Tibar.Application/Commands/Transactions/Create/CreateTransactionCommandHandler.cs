using MediatR;
using Tibar.Application.Common;
using Tibar.Application.Interfaces;
using Tibar.Domain.Entities;
using Tibar.Domain.Enums;
using Tibar.Domain.Exceptions;
using Tibar.Domain.Helpers;
using Tibar.Domain.ValueObjects;

namespace Tibar.Application.Commands.Transactions.Create;

public class CreateTransactionCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateTransactionCommand, Result<List<DTOs.TransactionDto>>>
{
    public async Task<Result<List<DTOs.TransactionDto>>> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var typeResult = ParseType(request.Type);
        if (!typeResult.IsValid)
            return Result.Failure<List<DTOs.TransactionDto>>(typeResult.Errors);

        var category = await context.Categories
            .FindAsync(new object[] { request.CategoryId }, cancellationToken);

        if (category is null || category.UserId != request.UserId)
            return Result.Failure<List<DTOs.TransactionDto>>("Categoria não encontrada.");

        var account = await context.Accounts
            .FindAsync(new object[] { request.AccountId }, cancellationToken);

        if (account is null || account.UserId != request.UserId)
            return Result.Failure<List<DTOs.TransactionDto>>("Conta não encontrada.");

        var amountResult = CreateAmount(request.Amount);
        if (!amountResult.IsValid)
            return Result.Failure<List<DTOs.TransactionDto>>(amountResult.Errors);

        var installments = request.Installments ?? 1;
        var installmentId = installments > 1 ? (Guid?)Guid.NewGuid() : null;
        var dates = InstallmentSchedule.GetDates(request.Date, installments);

        var transactions = dates
            .Select(date => new Transaction(
                request.Description,
                amountResult.Data! with { },
                typeResult.Data!,
                request.CategoryId,
                request.AccountId,
                request.UserId,
                date,
                installmentId))
            .ToList();

        context.Transactions.AddRange(transactions);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(transactions
            .Select(t => MapToDto(t, category.Name, account.Description))
            .ToList());
    }

    private static Result<TransactionType> ParseType(string type)
    {
        return type.ToLower() switch
        {
            "income" => Result.Success(TransactionType.Income),
            "expense" => Result.Success(TransactionType.Expense),
            _ => Result.Failure<TransactionType>("Tipo de transação inválido. Deve ser 'income' ou 'expense'.")
        };
    }

    private static Result<Money> CreateAmount(decimal value)
    {
        try
        {
            return Result.Success(Money.Create(value));
        }
        catch (DomainException ex)
        {
            return Result.Failure<Money>(ex.Message);
        }
    }

    private static DTOs.TransactionDto MapToDto(Transaction t, string categoryName, string accountName)
        => new(
            t.Id,
            t.Description,
            t.Amount.Amount,
            t.Amount.Currency,
            t.Type.ToString(),
            t.CategoryId,
            categoryName,
            t.AccountId,
            accountName,
            t.Date,
            t.InstallmentId,
            t.CreatedAt);
}
