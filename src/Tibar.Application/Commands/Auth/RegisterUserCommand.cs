using MediatR;
using Tibar.Application.Common;
using Tibar.Application.DTOs.Auth;

namespace Tibar.Application.Commands.Auth;

public record RegisterUserCommand(
    string Name,
    string Email,
    string Password) : IRequest<Result<TokenResponse>>;
