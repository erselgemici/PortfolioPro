using PortfolioPro.Application.Common.Abstractions;
using PortfolioPro.Infrastructure.Persistence;

namespace PortfolioPro.Infrastructure.Storage;

// Dosyaları StoredFiles tablosuna kaydediyor. Dışarıya /uploads/{klasör}/{dosya} adresinden sunuluyor (StoredFileEndpoints).
// NOT: Application katmanı sadece IFileStorage'ı bildiği için diskten veritabanına geçerken hiçbir handler değişmedi.
internal sealed class DatabaseFileStorage(AppDbContext db, TimeProvider time) : IFileStorage
{
    public async Task<string> SaveAsync(Stream content, string extension, string folder, CancellationToken ct)
    {
        using var memory = new MemoryStream();
        await content.CopyToAsync(memory, ct);

        // Dosya adını biz üretiyoruz, kullanıcının verdiği isim hiç kullanılmıyor
        var path = $"{folder}/{Guid.NewGuid():N}{extension}";

        db.Files.Add(new StoredFile
        {
            Path = path,
            ContentType = StoredFileEndpoints.GetContentType(extension),
            Data = memory.ToArray(),
            CreatedAt = time.GetUtcNow().UtcDateTime
        });
        await db.SaveChangesAsync(ct);

        return $"/uploads/{path}";
    }
}
