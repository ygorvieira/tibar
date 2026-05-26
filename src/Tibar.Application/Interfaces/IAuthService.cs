using Tibar.Application.DTOs.Auth;

namespace Tibar.Application.Interfaces;

public interface IAuthService
{
    Task<TokenResponse> RegisterAsync(string name, string email, string password, CancellationToken cancellationToken);
    Task<TokenResponse> LoginAsync(string email, string password, CancellationToken cancellationToken);
}
