namespace PortfolioPro.API.Middleware;

public static class SecurityHeaders
{
    // Her cevaba güvenlik header'ları ekliyor
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            var headers = context.Response.Headers;

            headers.XContentTypeOptions = "nosniff";             // tarayıcı içerik tipini tahmin etmesin
            headers.XFrameOptions = "DENY";                      // site iframe içine alınamasın (clickjacking)
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

            // API sadece JSON dönüyor, sayfa çizmiyor. Bu yüzden en sıkı CSP'yi verebiliriz.
            // Swagger arayüzü script çalıştırdığı için onu hariç tutuyoruz (sadece development'ta açık).
            if (context.Request.Path.StartsWithSegments("/api"))
                headers.ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'";

            await next();
        });

    public static IApplicationBuilder UseUploadsStaticFiles(this IApplicationBuilder app) =>
        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = ctx =>
            {
                // Dosya isimleri rastgele Guid, içerik hiç değişmiyor. Tarayıcı 30 gün önbellekte tutabilir.
                ctx.Context.Response.Headers.CacheControl = "public, max-age=2592000, immutable";
            }
        });
}
