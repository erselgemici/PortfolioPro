namespace PortfolioPro.Application.Common.Abstractions;

public interface IFileStorage
{
    // Dosyayı kaydedip public URL'ini döner (/uploads/images/xxx.webp gibi)
    Task<string> SaveAsync(Stream content, string extension, string folder, CancellationToken ct);
}
