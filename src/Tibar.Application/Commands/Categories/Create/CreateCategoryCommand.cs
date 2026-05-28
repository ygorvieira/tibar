using MediatR;
using Tibar.Application.Common;
using Tibar.Application.DTOs;

namespace Tibar.Application.Commands.Categories.Create;

public record CreateCategoryCommand(
    string Name,
    string Type,
    Guid UserId) : IRequest<Result<CategoryDto>>;
