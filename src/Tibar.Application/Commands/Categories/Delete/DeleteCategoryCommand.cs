using MediatR;
using Tibar.Application.Common;

namespace Tibar.Application.Commands.Categories.Delete;

public record DeleteCategoryCommand(Guid Id, Guid UserId) : IRequest<Result<Unit>>;
