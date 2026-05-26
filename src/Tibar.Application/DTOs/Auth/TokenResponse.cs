namespace Tibar.Application.DTOs.Auth;

public record TokenResponse(
    string Token,
    string Email,
    string Name,
    DateTime ExpiresAt);
