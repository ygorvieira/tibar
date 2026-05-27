using MediatR;
using Tibar.Application.Common;
using Tibar.Application.Interfaces;
using Tibar.Domain.Entities;
using Tibar.Domain.Enums;

namespace Tibar.Application.Commands.Categories;

public class CreateCategoryCommandHandler(IApplicationDbContext context) : IRequestHandler<CreateCategoryCommand, Result<DTOs.CategoryDto>>
{
    private readonly IApplicationDbContext _context = context;

    public async Task<Result<DTOs.CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var typeResult = ParseType(request.Type);
        if (!typeResult.IsValid)
            return Result.Failure<DTOs.CategoryDto>(typeResult.Errors);

        var category = new Category(request.Name, typeResult.Data!, request.UserId);

        _context.Categories.Add(category);
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
