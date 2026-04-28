using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EdTech.SheetIntegrator.Application.Common;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EdTech.SheetIntegrator.Api.Auth;

/// <summary>Mints HS256 JWTs for the Instructor role. Used by the dev-only token endpoint.</summary>
public sealed class JwtTokenService
{
    public const string InstructorRole = "Instructor";
    public const string InstructorPolicy = "Instructor";

    private readonly JwtOptions _options;
    private readonly IClock _clock;

    public JwtTokenService(IOptions<JwtOptions> options, IClock clock)
    {
        _options = options.Value;
        _clock = clock;
    }

    public string IssueInstructorToken(string subject)
    {
        var keyBytes = Encoding.UTF8.GetBytes(_options.SigningKey);
        var key = new SymmetricSecurityKey(keyBytes);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var now = _clock.UtcNow.UtcDateTime;
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, subject),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, InstructorRole),
            ],
            notBefore: now,
            expires: now.AddMinutes(_options.TokenLifetimeMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
