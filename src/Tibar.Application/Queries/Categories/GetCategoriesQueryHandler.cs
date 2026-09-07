using MediatR;
using Microsoft.EntityFrameworkCore;
using Tibar.Application.Common;
using Tibar.Application.Interfaces;

namespace Tibar.Application.Queries.Categories;

public class GetCategoriesQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetCategoriesQuery, Result<PagedResult<DTOs.CategoryDto>>>
{
    public async Task<Result<PagedResult<DTOs.CategoryDto>>> Handle(
        GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var query = context.Categories
            .Where(c => c.UserId == request.UserId);

        var totalCount = await query.CountAsync(cancellationToken);

        var dtos = await query
            .OrderBy(c => c.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new DTOs.CategoryDto(
                c.Id, c.Name, c.Type.ToString(), c.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success(new PagedResult<DTOs.CategoryDto>(dtos, totalCount, request.Page, request.PageSize));
    }
}
