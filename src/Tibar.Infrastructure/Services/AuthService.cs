using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IApplicationDbContext _context;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        UserManager<AppUser> userManager,
        IApplicationDbContext context,
        IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _context = context;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<TokenResponse> RegisterAsync(string name, string email, string password, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser is not null)
            throw new DomainException("Email already registered.");

        var appUser = new AppUser
        {
            UserName = email,
            Email = email,
            Name = name
        };

        var identityResult = await _userManager.CreateAsync(appUser, password);
        if (!identityResult.Succeeded)
        {
            var errors = string.Join("; ", identityResult.Errors.Select(e => e.Description));
            throw new DomainException(errors);
        }

        var domainUser = new User(name, email);
        _context.Users.Add(domainUser);
        await _context.SaveChangesAsync(cancellationToken);

        var (token, expiresAt) = GenerateJwtToken(domainUser.Id, email, name);

        return new TokenResponse(token, email, name, expiresAt);
    }

    public async Task<TokenResponse> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var appUser = await _userManager.FindByEmailAsync(email);
        if (appUser is null || !await _userManager.CheckPasswordAsync(appUser, password))
            throw new DomainException("Invalid email or password.");

        var domainUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        var userId = domainUser?.Id ?? appUser.Id;

        var (token, expiresAt) = GenerateJwtToken(userId, email, appUser.Name);

        return new TokenResponse(token, email, appUser.Name, expiresAt);
    }

    private (string token, DateTime expiresAt) GenerateJwtToken(Guid userId, string email, string name)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Key);
        var expiresAt = DateTime.UtcNow.AddHours(_jwtSettings.ExpirationInHours);

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
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return (tokenHandler.WriteToken(token), expiresAt);
    }
}
