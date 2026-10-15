using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PortfolioPro.Application.Common.Abstractions;
using PortfolioPro.Domain.Entities;
using PortfolioPro.Infrastructure.Identity;
using PortfolioPro.Infrastructure.Storage;

namespace PortfolioPro.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options), IAppDbContext
{
    public DbSet<SiteProfile> Profiles => Set<SiteProfile>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Experience> Experiences => Set<Experience>();
    public DbSet<Education> Educations => Set<Education>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

    // Application katmanı bunu görmüyor, sadece AuthService kullanıyor
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<StoredFile> Files => Set<StoredFile>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // NOT: Identity tablolarının ayarları base'de, bunu çağırmayı unutursak Identity tabloları hatalı oluşur
        base.OnModelCreating(builder);

        // Configurations klasöründeki tüm IEntityTypeConfiguration sınıflarını otomatik uygula
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
