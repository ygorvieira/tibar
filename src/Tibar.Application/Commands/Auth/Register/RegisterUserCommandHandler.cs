using MediatR;
using Tibar.Application.Common;
using Tibar.Application.Interfaces;

namespace Tibar.Application.Commands.Auth.Register;

public class RegisterUserCommandHandler(
    IAuthService authService) : IRequestHandler<RegisterUserCommand, Result<DTOs.Auth.TokenResponse>>
{
    public async Task<Result<DTOs.Auth.TokenResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request.Name, request.Email, request.Password, cancellationToken);
        return Result.Success(result);
    }
}
