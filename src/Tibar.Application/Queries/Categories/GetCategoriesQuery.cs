using MediatR;
using Tibar.Application.Common;
using Tibar.Application.DTOs;

namespace Tibar.Application.Queries.Categories;

public record GetCategoriesQuery(Guid UserId) : IRequest<Result<IEnumerable<CategoryDto>>>;
