using MediatR;
using Tibar.Application.Common;
using Tibar.Application.Interfaces;

namespace Tibar.Application.Commands.Auth;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<DTOs.Auth.TokenResponse>>
{
    private readonly IAuthService _authService;

    public RegisterUserCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<DTOs.Auth.TokenResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request.Name, request.Email, request.Password, cancellationToken);
        return Result.Success(result);
    }
}
