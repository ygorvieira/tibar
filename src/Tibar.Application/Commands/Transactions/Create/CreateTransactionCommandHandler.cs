using MediatR;
using Tibar.Application.Common;
using Tibar.Application.Interfaces;
using Tibar.Domain.Entities;
using Tibar.Domain.Enums;
using Tibar.Domain.Exceptions;
using Tibar.Domain.ValueObjects;

namespace Tibar.Application.Commands.Transactions.Create;

public class CreateTransactionCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateTransactionCommand, Result<DTOs.TransactionDto>>
{
    public async Task<Result<DTOs.TransactionDto>> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var typeResult = ParseType(request.Type);
        if (!typeResult.IsValid)
            return Result.Failure<DTOs.TransactionDto>(typeResult.Errors);

        var category = await context.Categories
            .FindAsync(new object[] { request.CategoryId }, cancellationToken);

        if (category is null || category.UserId != request.UserId)
            return Result.Failure<DTOs.TransactionDto>("Category not found.");

        var amountResult = CreateAmount(request.Amount);
        if (!amountResult.IsValid)
            return Result.Failure<DTOs.TransactionDto>(amountResult.Errors);

        var transaction = new Transaction(
            request.Description,
            amountResult.Data!,
            typeResult.Data!,
            request.CategoryId,
            request.UserId,
            request.Date);

        context.Transactions.Add(transaction);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToDto(transaction, category.Name));
    }

    private static Result<TransactionType> ParseType(string type)
    {
        return type.ToLower() switch
        {
            "income" => Result.Success(TransactionType.Income),
            "expense" => Result.Success(TransactionType.Expense),
            _ => Result.Failure<TransactionType>("Invalid transaction type. Must be 'income' or 'expense'.")
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

    private static DTOs.TransactionDto MapToDto(Transaction t, string categoryName)
        => new(
            t.Id,
            t.Description,
            t.Amount.Amount,
            t.Amount.Currency,
            t.Type.ToString(),
            t.CategoryId,
            categoryName,
            t.Date,
            t.CreatedAt);
}
