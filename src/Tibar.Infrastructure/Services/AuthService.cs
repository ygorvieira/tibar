using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Tibar.Application.DTOs.Auth;
using Tibar.Application.Interfaces;
using Tibar.Domain.Entities;
using Tibar.Domain.Exceptions;
using Tibar.Infrastructure.Identity;

namespace Tibar.Infrastructure.Services;

public class JwtSettings
{
    public string Key { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int ExpirationInHours { get; set; } = 8;
}

public class AuthService(
    UserManager<AppUser> userManager,
    IApplicationDbContext context,
    IOptions<JwtSettings> jwtSettings,
    ILogger<AuthService> logger) : IAuthService
{

    public async Task<TokenResponse> RegisterAsync(string name, string email, string password, CancellationToken cancellationToken)
    {
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is not null)
            throw new DomainException("E-mail já cadastrado.", 409);

        var appUser = new AppUser
        {
            UserName = email,
            Email = email,
            Name = name
        };

        var identityResult = await userManager.CreateAsync(appUser, password);
        if (!identityResult.Succeeded)
        {
            logger.LogWarning("Falha no registro: {Errors}",
                string.Join("; ", identityResult.Errors.Select(e => e.Description)));
            throw new DomainException("Falha no registro. Verifique suas informações.");
        }

        var domainUser = new User(name, email);
        context.Users.Add(domainUser);
        await context.SaveChangesAsync(cancellationToken);

        var (token, expiresAt) = GenerateJwtToken(domainUser.Id, email, name);

        return new TokenResponse(token, email, name, expiresAt);
    }

    public async Task<TokenResponse> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var appUser = await userManager.FindByEmailAsync(email);
        if (appUser is null)
            throw new DomainException("E-mail ou senha inválidos.");

        if (await userManager.IsLockedOutAsync(appUser))
            throw new DomainException("Conta temporariamente bloqueada. Tente novamente mais tarde.", 429);

        if (!await userManager.CheckPasswordAsync(appUser, password))
        {
            await userManager.AccessFailedAsync(appUser);
            throw new DomainException("E-mail ou senha inválidos.");
        }

        await userManager.ResetAccessFailedCountAsync(appUser);

        var domainUser = await context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        var userId = domainUser?.Id ?? appUser.Id;

        var (token, expiresAt) = GenerateJwtToken(userId, email, appUser.Name);

        return new TokenResponse(token, email, appUser.Name, expiresAt);
    }

    private (string token, DateTime expiresAt) GenerateJwtToken(Guid userId, string email, string name)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(jwtSettings.Value.Key);
        var expiresAt = DateTime.UtcNow.AddHours(jwtSettings.Value.ExpirationInHours);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, name),
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = jwtSettings.Value.Issuer,
            Audience = jwtSettings.Value.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return (tokenHandler.WriteToken(token), expiresAt);
    }
}
