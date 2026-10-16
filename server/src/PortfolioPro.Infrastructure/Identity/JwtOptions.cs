using System.ComponentModel.DataAnnotations;

namespace PortfolioPro.Infrastructure.Identity;

// appsettings.json'daki "Jwt" bölümü otomatik olarak bu sınıfa dolduruluyor (Options pattern)
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required] public string Issuer { get; init; } = string.Empty;
    [Required] public string Audience { get; init; } = string.Empty;

    // NOT: Key repoda YOK. Geliştirmede user-secrets, sunucuda ortam değişkeni (Jwt__Key) ile veriliyor
    [Required, MinLength(32)] public string Key { get; init; } = string.Empty;

    // Access token kısa ömürlü, refresh token uzun ömürlü
    [Range(1, 120)] public int AccessTokenMinutes { get; init; } = 15;
    [Range(1, 60)] public int RefreshTokenDays { get; init; } = 7;
}
