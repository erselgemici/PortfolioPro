using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortfolioPro.Domain.Entities;
using PortfolioPro.Infrastructure.Identity;

namespace PortfolioPro.Infrastructure.Persistence.Configurations;

// NOT: Tablo ayarlarını (uzunluk, index vb.) entity'lerin üstüne attribute olarak yazmak yerine burada tutuyorum.
// Böylece Domain katmanı EF Core'u hiç bilmiyor.

internal sealed class SiteProfileConfiguration : IEntityTypeConfiguration<SiteProfile>
{
    public void Configure(EntityTypeBuilder<SiteProfile> b)
    {
        b.ToTable("Profiles");
        b.Property(x => x.FullName).HasMaxLength(100).IsRequired();
        b.Localized(x => x.Title, 100);
        b.Localized(x => x.Headline, 300);
        b.Localized(x => x.About, 4000);
        b.Property(x => x.Location).HasMaxLength(100);
        b.Property(x => x.Email).HasMaxLength(150);
        b.Property(x => x.AvatarUrl).HasMaxLength(500);
        b.Property(x => x.CvUrlTr).HasMaxLength(500);
        b.Property(x => x.CvUrlEn).HasMaxLength(500);
        b.Property(x => x.GithubUrl).HasMaxLength(300);
        b.Property(x => x.LinkedinUrl).HasMaxLength(300);
    }
}

internal sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> b)
    {
        b.Property(x => x.Slug).HasMaxLength(80).IsRequired();
        b.HasIndex(x => x.Slug).IsUnique(); // aynı slug iki kez olamaz, veritabanı seviyesinde de garanti

        b.Localized(x => x.Title, 150);
        b.Localized(x => x.Summary, 400);
        b.Localized(x => x.Content); // markdown, uzunluk sınırı yok -> text

        // Enum'ı sayı yerine yazı olarak saklıyoruz, veritabanına bakınca "Web" görmek "1"den daha anlaşılır
        b.Property(x => x.Category).HasConversion<string>().HasMaxLength(20);

        b.Property(x => x.CoverImageUrl).HasMaxLength(500);
        b.Property(x => x.GithubUrl).HasMaxLength(300);
        b.Property(x => x.LiveUrl).HasMaxLength(300);
        b.Property(x => x.StoreUrl).HasMaxLength(300);

        // Public sayfada en sık çalışan sorgu "yayında olanlar" sorgusu
        b.HasIndex(x => new { x.IsPublished, x.DisplayOrder });
    }
}

internal sealed class ExperienceConfiguration : IEntityTypeConfiguration<Experience>
{
    public void Configure(EntityTypeBuilder<Experience> b)
    {
        b.Property(x => x.Company).HasMaxLength(150).IsRequired();
        b.Localized(x => x.Position, 150);
        b.Localized(x => x.Description, 3000);
        b.Property(x => x.Location).HasMaxLength(100);
    }
}

internal sealed class EducationConfiguration : IEntityTypeConfiguration<Education>
{
    public void Configure(EntityTypeBuilder<Education> b)
    {
        b.Property(x => x.School).HasMaxLength(150).IsRequired();
        b.Localized(x => x.Department, 150);
        b.Localized(x => x.Degree, 100);
        b.Localized(x => x.Description, 1000);
    }
}

internal sealed class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> b)
    {
        b.Property(x => x.Name).HasMaxLength(50).IsRequired();
        b.HasIndex(x => x.Name).IsUnique();
        b.Property(x => x.Category).HasConversion<string>().HasMaxLength(20);
    }
}

internal sealed class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> b)
    {
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.Issuer).HasMaxLength(100).IsRequired();
        b.Property(x => x.CredentialUrl).HasMaxLength(300);
    }
}

internal sealed class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
{
    public void Configure(EntityTypeBuilder<ContactMessage> b)
    {
        b.ToTable("ContactMessages");
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.Property(x => x.Email).HasMaxLength(150).IsRequired();
        b.Property(x => x.Subject).HasMaxLength(150);
        b.Property(x => x.Body).HasMaxLength(4000).IsRequired();
        b.HasIndex(x => new { x.IsRead, x.CreatedAt });
    }
}

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.Property(x => x.TokenHash).HasMaxLength(64).IsRequired(); // SHA256 hex = 64 karakter
        b.Property(x => x.ReplacedByTokenHash).HasMaxLength(64);
        b.HasIndex(x => x.TokenHash).IsUnique();

        b.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade); // kullanıcı silinirse token'ları da silinsin
    }
}

internal sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> b)
    {
        b.Property(x => x.FullName).HasMaxLength(100);
    }
}
