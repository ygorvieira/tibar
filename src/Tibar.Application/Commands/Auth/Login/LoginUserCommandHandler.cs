using MediatR;
using Tibar.Application.Common;
using Tibar.Application.Interfaces;

namespace Tibar.Application.Commands.Auth.Login;

public class LoginUserCommandHandler(
    IAuthService authService) : IRequestHandler<LoginUserCommand, Result<DTOs.Auth.TokenResponse>>
{
    public async Task<Result<DTOs.Auth.TokenResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request.Email, request.Password, cancellationToken);
        return Result.Success(result);
    }
}
