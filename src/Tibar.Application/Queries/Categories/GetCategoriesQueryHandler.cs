using MediatR;
using Microsoft.EntityFrameworkCore;
using Tibar.Application.Common;
using Tibar.Application.Interfaces;

namespace Tibar.Application.Queries.Categories;

public class GetCategoriesQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetCategoriesQuery, Result<IEnumerable<DTOs.CategoryDto>>>
{
    public async Task<Result<IEnumerable<DTOs.CategoryDto>>> Handle(
        GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var dtos = await context.Categories
            .Where(c => c.UserId == request.UserId)
            .OrderBy(c => c.Name)
            .Select(c => new DTOs.CategoryDto(
                c.Id, c.Name, c.Type.ToString(), c.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<DTOs.CategoryDto>>(dtos);
    }
}
