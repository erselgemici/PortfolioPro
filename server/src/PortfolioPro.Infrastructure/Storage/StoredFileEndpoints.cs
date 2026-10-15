using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using PortfolioPro.Infrastructure.Persistence;

namespace PortfolioPro.Infrastructure.Storage;

public static class StoredFileEndpoints
{
    public static IEndpointRouteBuilder MapStoredFiles(this IEndpointRouteBuilder app)
    {
        app.MapGet("/uploads/{folder}/{fileName}", async (string folder, string fileName, AppDbContext db, HttpContext context, CancellationToken ct) =>
            {
                var path = $"{folder}/{fileName}";

                var file = await db.Files
                    .AsNoTracking()
                    .Where(f => f.Path == path)
                    .Select(f => new { f.Data, f.ContentType })
                    .FirstOrDefaultAsync(ct);

                if (file is null)
                    return Results.NotFound();

                // Dosya isimleri rastgele Guid, içerik hiç değişmiyor. Tarayıcı 30 gün önbellekte tutabilir.
                context.Response.Headers.CacheControl = "public, max-age=2592000, immutable";
                return Results.File(file.Data, file.ContentType);
            })
            .ExcludeFromDescription();

        return app;
    }

    public static string GetContentType(string extension) => extension switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".webp" => "image/webp",
        ".pdf" => "application/pdf",
        _ => "application/octet-stream"
    };
}
