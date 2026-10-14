namespace PortfolioPro.Infrastructure.Identity;

// Refresh token'ın kendisini değil HASH'ini saklıyoruz.
// NOT: Veritabanı sızsa bile saldırgan bu hash'lerle oturum açamaz, tıpkı şifrelerde olduğu gibi.
public class RefreshToken
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    // Rotation: her yenilemede eski token iptal edilip yerine yenisi veriliyor, hangisiyle değiştiğini tutuyoruz
    public string? ReplacedByTokenHash { get; set; }

    public AppUser? User { get; set; }
}
