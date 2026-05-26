using MediatR;
using Tibar.Application.Common;
using Tibar.Application.Interfaces;
using Tibar.Domain.Exceptions;
using Tibar.Domain.ValueObjects;

namespace Tibar.Application.Commands.Transactions;

public class UpdateTransactionCommandHandler : IRequestHandler<UpdateTransactionCommand, Result<DTOs.TransactionDto>>
{
    private readonly IApplicationDbContext _context;

    public UpdateTransactionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<DTOs.TransactionDto>> Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _context.Transactions
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (transaction is null || transaction.UserId != request.UserId)
            return Result.Failure<DTOs.TransactionDto>("Transaction not found.");

        var category = await _context.Categories
            .FindAsync(new object[] { request.CategoryId }, cancellationToken);

        if (category is null)
            return Result.Failure<DTOs.TransactionDto>("Category not found.");

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
        transaction.UpdateDate(request.Date);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new DTOs.TransactionDto(
            transaction.Id,
            transaction.Description,
            transaction.Amount.Amount,
            transaction.Amount.Currency,
            transaction.Type.ToString(),
            transaction.CategoryId,
            category.Name,
            transaction.Date,
            transaction.CreatedAt));
    }
}
