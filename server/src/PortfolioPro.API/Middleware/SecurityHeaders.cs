namespace PortfolioPro.API.Middleware;

public static partial class SecurityHeaders
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

            // API sadece JSON dönüyor, sayfa göstermiyor. Bu yüzden en sıkı güvenlik ayarını (CSP) verebiliriz.
            // Swagger arayüzü script çalıştırdığı için onu hariç tutuyoruz (sadece development'ta açık).
            if (context.Request.Path.StartsWithSegments("/api"))
                headers.ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'";
            else if (!context.Request.Path.StartsWithSegments("/swagger"))
                headers.ContentSecurityPolicy = PageCsp;

            await next();
        });

    // Angular sayfaları için CSP: tarayıcıya sadece kendi sitemizden gelen script'leri çalıştırmasını söylüyor.
    // index.html'deki tema script'i sayfanın içine yazılı olduğu için onu hash'i ile izinli listeye ekledim.
    // NOT: index.html'deki o script değişirse bu hash'i de güncellemek gerekiyor (README'de anlatıldı).
    private const string PageCsp =
        "default-src 'self'; script-src 'self' 'sha256-O1i3i81KmXoG2Op2VbIN0HPTtNW8n13R3HXSMKCtL5M='; " +
        "style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self'; connect-src 'self'; " +
        "frame-ancestors 'none'; base-uri 'self'; form-action 'self'; object-src 'none'";

    public static IApplicationBuilder UseSpaStaticFiles(this IApplicationBuilder app)
    {
        app.UseDefaultFiles();
        return app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = ctx =>
            {
                var name = ctx.File.Name;

                // index.html her seferinde kontrol edilsin, yoksa yeni sürüm yayınlanınca eski sayfa açılır.
                // İsminde hash olan dosyalar (main-ABC123.js) hiç değişmez, 1 yıl önbellekte kalabilir.
                ctx.Context.Response.Headers.CacheControl = name == "index.html"
                    ? "no-cache"
                    : HashedFileName().IsMatch(name)
                        ? "public, max-age=31536000, immutable"
                        : "public, max-age=86400";
            }
        });
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"-[A-Z0-9]{8,}\.(js|css)$")]
    private static partial System.Text.RegularExpressions.Regex HashedFileName();
}
