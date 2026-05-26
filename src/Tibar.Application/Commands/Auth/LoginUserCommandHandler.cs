using MediatR;
using Tibar.Application.Common;
using Tibar.Application.Interfaces;

namespace Tibar.Application.Commands.Auth;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Result<DTOs.Auth.TokenResponse>>
{
    private readonly IAuthService _authService;

    public LoginUserCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<DTOs.Auth.TokenResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request.Email, request.Password, cancellationToken);
        return Result.Success(result);
    }
}
