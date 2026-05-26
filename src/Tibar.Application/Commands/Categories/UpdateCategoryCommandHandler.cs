using MediatR;
using Tibar.Application.Common;
using Tibar.Application.Interfaces;
using Tibar.Domain.Enums;

namespace Tibar.Application.Commands.Categories;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result<DTOs.CategoryDto>>
{
    private readonly IApplicationDbContext _context;

    public UpdateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<DTOs.CategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (category is null || category.UserId != request.UserId)
            return Result.Failure<DTOs.CategoryDto>("Category not found.");

        var typeResult = ParseType(request.Type);
        if (!typeResult.IsValid)
            return Result.Failure<DTOs.CategoryDto>(typeResult.Errors);

        category.Update(request.Name, typeResult.Data!);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new DTOs.CategoryDto(
            category.Id,
            category.Name,
            category.Type.ToString(),
            category.CreatedAt));
    }

    private static Result<TransactionType> ParseType(string type)
    {
        return type.ToLower() switch
        {
            "income" => Result.Success(TransactionType.Income),
            "expense" => Result.Success(TransactionType.Expense),
            _ => Result.Failure<TransactionType>("Invalid category type. Must be 'income' or 'expense'.")
        };
    }
}
