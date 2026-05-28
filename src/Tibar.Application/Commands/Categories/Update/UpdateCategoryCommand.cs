using MediatR;
using Tibar.Application.Common;
using Tibar.Application.DTOs;

namespace Tibar.Application.Commands.Categories.Update;

public record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string Type,
    Guid UserId) : IRequest<Result<CategoryDto>>;
