using System.ComponentModel.DataAnnotations;

namespace EdTech.SheetIntegrator.Api.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    [Required]
    [MinLength(32, ErrorMessage = "JWT signing key must be at least 32 bytes (256 bits) for HS256.")]
    public string SigningKey { get; init; } = string.Empty;

    [Range(1, 24 * 60)]
    public int TokenLifetimeMinutes { get; init; } = 60;
}
