using System.IO.Compression;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using PortfolioPro.API.Middleware;
using PortfolioPro.Infrastructure.Persistence;

namespace PortfolioPro.API.Extensions;

public static class ApiServiceExtensions
{
    public const string AdminPolicy = "Admin";
    public const string AuthRateLimit = "auth";
    public const string ContactRateLimit = "contact";
    public const string PublicCachePolicy = "public";
    public const string PortfolioCacheTag = "portfolio";

    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Enum'lar JSON'da sayı yerine yazı olarak gitsin: "category": "Web"
        services.ConfigureHttpJsonOptions(o =>
            o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddOpenApi();

        services.AddAuthorizationBuilder()
            .AddPolicy(AdminPolicy, p => p.RequireAuthenticatedUser().RequireRole(DbInitializer.AdminRole));

        AddCors(services, configuration);
        AddRateLimiting(services);

        // Public endpoint'lerin cevabı 10 dk önbellekte. Admin bir şey değiştirince tag ile temizleniyor.
        services.AddOutputCache(o =>
            o.AddPolicy(PublicCachePolicy, p => p.Expire(TimeSpan.FromMinutes(10)).Tag(PortfolioCacheTag)));

        services.AddResponseCompression(o =>
        {
            o.EnableForHttps = true;
            o.Providers.Add<BrotliCompressionProvider>();
            o.Providers.Add<GzipCompressionProvider>();
        });
        services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);

        // NOT: Site, araya giren bir sunucunun (proxy: Render ya da Caddy) arkasında çalışıyor.
        // Ziyaretçinin gerçek IP'si X-Forwarded-For adlı header'da geliyor. Bunu okumazsak bütün istekler
        // proxy'nin IP'sinden geliyor gibi görünür ve rate limit tüm ziyaretçileri tek kişi sayar.
        services.Configure<ForwardedHeadersOptions>(o =>
        {
            o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            // Bu header'a her proxy kendi IP'sini ekliyor, o yüzden sondan sayıyoruz. Caddy'de 1 yeterli,
            // Render'da 2 olmalı (ReverseProxy__ForwardLimit=2 ortam değişkeniyle).
            // Gereğinden büyük verirsem kullanıcı header'a sahte IP yazıp limiti atlatabilir.
            o.ForwardLimit = configuration.GetValue("ReverseProxy:ForwardLimit", 1);
            o.KnownNetworks.Clear();
            o.KnownProxies.Clear();
        });

        return services;
    }

    private static void AddCors(IServiceCollection services, IConfiguration configuration)
    {
        // Production'da Angular ve API aynı domainden yayınlandığı için CORS'a gerek kalmıyor.
        // Farklı domainden yayınlanırsa izinli adresler buraya yazılıyor.
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(o => o.AddDefaultPolicy(p =>
        {
            if (origins.Length == 0) return;

            p.WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials(); // refresh token cookie'si için gerekli
        }));
    }

    private static void AddRateLimiting(IServiceCollection services)
    {
        services.AddRateLimiter(o =>
        {
            o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Genel limit: IP başına dakikada 120 istek
            o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
                RateLimitPartition.GetFixedWindowLimiter(ClientIp(ctx), _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 120,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));

            // Login: IP başına dakikada 5 deneme. Kaba kuvvet (brute force) saldırısını yavaşlatır.
            o.AddPolicy(AuthRateLimit, ctx =>
                RateLimitPartition.GetFixedWindowLimiter(ClientIp(ctx), _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));

            // İletişim formu: IP başına 10 dakikada 3 mesaj. Spam'i engeller.
            o.AddPolicy(ContactRateLimit, ctx =>
                RateLimitPartition.GetFixedWindowLimiter(ClientIp(ctx), _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 3,
                    Window = TimeSpan.FromMinutes(10),
                    QueueLimit = 0
                }));

            o.OnRejected = async (context, ct) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();

                await Results.Problem(
                        statusCode: StatusCodes.Status429TooManyRequests,
                        detail: "Çok fazla istek gönderdiniz. Lütfen biraz bekleyip tekrar deneyin.",
                        extensions: new Dictionary<string, object?> { ["code"] = "rate_limited" })
                    .ExecuteAsync(context.HttpContext);
            };
        });
    }

    private static string ClientIp(HttpContext ctx) =>
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}
