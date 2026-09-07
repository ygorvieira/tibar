using MediatR;
using Tibar.Application.Common;
using Tibar.Application.DTOs;

namespace Tibar.Application.Queries.Categories;

public record GetCategoriesQuery(Guid UserId, int Page = 1, int PageSize = 50) : IRequest<Result<PagedResult<CategoryDto>>>;
