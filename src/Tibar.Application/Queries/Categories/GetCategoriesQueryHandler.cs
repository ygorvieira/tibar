using MediatR;
using Microsoft.EntityFrameworkCore;
using Tibar.Application.Common;
using Tibar.Application.Interfaces;

namespace Tibar.Application.Queries.Categories;

public class GetCategoriesQueryHandler
    : IRequestHandler<GetCategoriesQuery, Result<IEnumerable<DTOs.CategoryDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<DTOs.CategoryDto>>> Handle(
        GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _context.Categories
            .Where(c => c.UserId == request.UserId)
            .Select(c => new DTOs.CategoryDto(
                c.Id,
                c.Name,
                c.Type.ToString(),
                c.CreatedAt))
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<DTOs.CategoryDto>>(categories);
    }
}
