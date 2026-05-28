using MediatR;
using Tibar.Application.Common;
using Tibar.Application.DTOs.Auth;

namespace Tibar.Application.Commands.Auth.Login;

public record LoginUserCommand(
    string Email,
    string Password) : IRequest<Result<TokenResponse>>;
