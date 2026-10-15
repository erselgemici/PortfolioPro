using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PortfolioPro.Infrastructure.Storage;

// Yüklenen görseller ve PDF'ler veritabanında tutuluyor.
// NOT: Ücretsiz hosting'lerde (Render vb.) disk kalıcı değil, her deploy'da siliniyor.
// Portfolyo sitesinde dosya sayısı az ve boyutları küçük (en fazla 5 MB), veritabanında tutmak yeterli.
public sealed class StoredFile
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Path { get; set; } = default!;          // "images/3f2a...webp" -> URL'de /uploads/ sonrası
    public string ContentType { get; set; } = default!;
    public byte[] Data { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}

internal sealed class StoredFileConfiguration : IEntityTypeConfiguration<StoredFile>
{
    public void Configure(EntityTypeBuilder<StoredFile> b)
    {
        b.ToTable("StoredFiles");
        b.Property(x => x.Path).HasMaxLength(200).IsRequired();
        b.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
        b.HasIndex(x => x.Path).IsUnique();   // dosya her zaman Path ile aranıyor
    }
}
