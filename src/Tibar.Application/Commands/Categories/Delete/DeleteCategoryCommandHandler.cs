using MediatR;
using Tibar.Application.Common;
using Tibar.Application.Interfaces;

namespace Tibar.Application.Commands.Categories.Delete;

public class DeleteCategoryCommandHandler(
    IApplicationDbContext context) : IRequestHandler<DeleteCategoryCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await context.Categories
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (category is null || category.UserId != request.UserId)
            return Result.Failure<Unit>("Category not found.");

        context.Categories.Remove(category);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(Unit.Value);
    }
}
