using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PortfolioPro.Application.Common;
using PortfolioPro.Application.Common.Abstractions;
using PortfolioPro.Application.Common.Models;
using PortfolioPro.Infrastructure.Persistence;

namespace PortfolioPro.Infrastructure.Identity;

internal sealed class AuthService(
    UserManager<AppUser> userManager,
    AppDbContext db,
    IOptions<JwtOptions> jwtOptions,
    TimeProvider time,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly JwtOptions _jwt = jwtOptions.Value;

    // NOT: "Kullanıcı bulunamadı" ve "şifre yanlış" için AYNI mesajı dönüyoruz.
    // Farklı mesaj dönersek saldırgan hangi e-postanın sistemde kayıtlı olduğunu öğrenebilir.
    private static readonly Error InvalidCredentials =
        Error.Unauthorized("auth.invalid_credentials", "E-posta veya şifre hatalı.");

    private static readonly Error InvalidRefreshToken =
        Error.Unauthorized("auth.invalid_refresh_token", "Oturumun süresi doldu, lütfen tekrar giriş yapın.");

    public async Task<Result<AuthTokens>> LoginAsync(string email, string password, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return InvalidCredentials;

        // Identity'nin lockout özelliği: 5 hatalı denemeden sonra hesap 15 dk kilitleniyor (ayarı DI'da)
        if (await userManager.IsLockedOutAsync(user))
        {
            logger.LogWarning("Kilitli hesaba giriş denemesi: {UserId}", user.Id);
            return Error.Unauthorized("auth.locked_out", "Çok fazla hatalı deneme yapıldı. Lütfen daha sonra tekrar deneyin.");
        }

        if (!await userManager.CheckPasswordAsync(user, password))
        {
            await userManager.AccessFailedAsync(user); // hatalı deneme sayacını artır
            logger.LogWarning("Hatalı şifre denemesi: {UserId}", user.Id);
            return InvalidCredentials;
        }

        await userManager.ResetAccessFailedCountAsync(user);
        return await IssueTokensAsync(user, ct);
    }

    public async Task<Result<AuthTokens>> RefreshAsync(string refreshToken, CancellationToken ct)
    {
        var now = time.GetUtcNow().UtcDateTime;
        var hash = Hash(refreshToken);

        var stored = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);
        if (stored is null)
            return InvalidRefreshToken;

        // NOT: Reuse detection. İptal edilmiş bir token tekrar kullanılıyorsa, token çalınmış olabilir.
        // Önlem olarak o kullanıcının TÜM aktif oturumlarını kapatıyoruz.
        if (stored.RevokedAt is not null)
        {
            logger.LogWarning("İptal edilmiş refresh token tekrar kullanıldı, tüm oturumlar kapatılıyor: {UserId}", stored.UserId);
            await RevokeAllAsync(stored.UserId, now, ct);
            return InvalidRefreshToken;
        }

        if (stored.ExpiresAt <= now)
            return InvalidRefreshToken;

        var user = await userManager.FindByIdAsync(stored.UserId.ToString());
        if (user is null || await userManager.IsLockedOutAsync(user))
            return InvalidRefreshToken;

        // Rotation: eskiyi iptal et, yenisini ver
        var tokens = await IssueTokensAsync(user, ct, replacing: stored);
        return tokens;
    }

    public async Task RevokeAsync(string refreshToken, CancellationToken ct)
    {
        var hash = Hash(refreshToken);
        var now = time.GetUtcNow().UtcDateTime;

        await db.RefreshTokens
            .Where(t => t.TokenHash == hash && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now), ct);
    }

    public async Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Error.NotFound("auth.user_not_found", "Kullanıcı bulunamadı.");

        var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
        {
            var message = result.Errors.FirstOrDefault()?.Description ?? "Şifre değiştirilemedi.";
            return Error.Validation("auth.change_password_failed", message);
        }

        // Şifre değişince açık olan tüm oturumları kapatıyoruz, kullanıcı yeni şifreyle tekrar giriş yapacak
        await RevokeAllAsync(user.Id, time.GetUtcNow().UtcDateTime, ct);
        return Result.Success();
    }

    // ---------------- yardımcı metotlar ----------------

    private async Task<AuthTokens> IssueTokensAsync(AppUser user, CancellationToken ct, RefreshToken? replacing = null)
    {
        var now = time.GetUtcNow().UtcDateTime;
        var roles = await userManager.GetRolesAsync(user);

        var accessExpires = now.AddMinutes(_jwt.AccessTokenMinutes);
        var accessToken = CreateAccessToken(user, roles, now, accessExpires);

        var refreshToken = GenerateRefreshToken();
        var refreshHash = Hash(refreshToken);
        var refreshExpires = now.AddDays(_jwt.RefreshTokenDays);

        if (replacing is not null)
        {
            replacing.RevokedAt = now;
            replacing.ReplacedByTokenHash = refreshHash;
        }

        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshHash,
            CreatedAt = now,
            ExpiresAt = refreshExpires
        });

        // Süresi çoktan dolmuş eski kayıtları temizle, tablo sonsuza kadar büyümesin
        await db.RefreshTokens
            .Where(t => t.UserId == user.Id && t.ExpiresAt < now.AddDays(-1))
            .ExecuteDeleteAsync(ct);

        await db.SaveChangesAsync(ct);

        return new AuthTokens(
            accessToken,
            accessExpires,
            refreshToken,
            refreshExpires,
            new AdminUserDto(user.Id, user.Email ?? string.Empty, user.FullName));
    }

    private string CreateAccessToken(AppUser user, IEnumerable<string> roles, DateTime now, DateTime expires)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new("name", user.FullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roles.Select(r => new Claim("role", r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _jwt.Issuer,
            Audience = _jwt.Audience,
            NotBefore = now,
            IssuedAt = now,
            Expires = expires,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };

        // NOT: .NET 8 ile JsonWebTokenHandler, eski JwtSecurityTokenHandler'ın yerini aldı (daha hızlı)
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    private async Task RevokeAllAsync(Guid userId, DateTime now, CancellationToken ct) =>
        await db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now), ct);

    // Tahmin edilemez rastgele değer. Random sınıfı DEĞİL, kriptografik RandomNumberGenerator kullanılmalı
    private static string GenerateRefreshToken() =>
        Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));

    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
