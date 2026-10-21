using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TunetalkApi.Application.Identity;

namespace TunetalkApi.Infrastructure.Identity;

public sealed class JwtTokenIssuer(
    JwtSettings settings,
    TimeProvider timeProvider) : IJwtTokenIssuer
{
    public AuthenticationResponse Create(string userId, string email, string displayName)
    {
        var issuedAt = timeProvider.GetUtcNow();
        var expiresAt = issuedAt.AddMinutes(settings.AccessTokenMinutes);
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey)),
            SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, displayName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var token = new JwtSecurityToken(
            settings.Issuer,
            settings.Audience,
            claims,
            issuedAt.UtcDateTime,
            expiresAt.UtcDateTime,
            credentials);

        return new AuthenticationResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt,
            email,
            displayName);
    }
}
