using Microsoft.AspNetCore.Hosting;
using PortfolioPro.Application.Common.Abstractions;

namespace PortfolioPro.Infrastructure.Storage;

// Dosyaları API'nin wwwroot/uploads klasörüne kaydediyor.
// NOT: İleride Azure Blob ya da S3'e geçmek istersem sadece bu sınıfın yerine yenisini yazmam yeterli,
// Application katmanı IFileStorage'ı bildiği için hiçbir handler değişmez.
internal sealed class LocalFileStorage(IWebHostEnvironment env) : IFileStorage
{
    public async Task<string> SaveAsync(Stream content, string extension, string folder, CancellationToken ct)
    {
        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var directory = Path.Combine(webRoot, "uploads", folder);
        Directory.CreateDirectory(directory);

        // Dosya adını biz üretiyoruz. Kullanıcının verdiği adı kullanırsak "../../appsettings.json"
        // gibi bir isimle başka klasöre yazmaya çalışabilir (path traversal)
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(directory, fileName);

        await using var stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
        await content.CopyToAsync(stream, ct);

        return $"/uploads/{folder}/{fileName}";
    }
}
