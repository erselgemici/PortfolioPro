using PortfolioPro.API.Endpoints;
using PortfolioPro.API.Extensions;
using PortfolioPro.API.Middleware;
using PortfolioPro.Application;
using PortfolioPro.Infrastructure;
using PortfolioPro.Infrastructure.Persistence;
using PortfolioPro.Infrastructure.Storage;

var builder = WebApplication.CreateBuilder(args);

// NOT: Render gibi platformlar uygulamanın dinleyeceği portu PORT ortam değişkeniyle veriyor
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ---------------- Servisler (DI) ----------------
// NOT: Her katman kendi servislerini kendi DependencyInjection sınıfında kaydediyor,
// Program.cs sadece bunları çağırıyor. Böylece bu dosya kısa ve okunur kalıyor.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

await app.InitializeDatabaseAsync();

// ---------------- Middleware pipeline ----------------
// NOT: Sıra önemli! İstek yukarıdan aşağı, cevap aşağıdan yukarı geçiyor.

app.UseForwardedHeaders();      // proxy (Caddy / Render) arkasında gerçek IP ve https bilgisini al (rate limit IP'ye göre çalışıyor)
app.UseExceptionHandler();      // yakalanmayan hataları ProblemDetails JSON'a çevir

if (!app.Environment.IsDevelopment())
    app.UseHsts();

app.UseSecurityHeaders();
app.UseResponseCompression();
app.UseSpaStaticFiles();        // Angular'ın derlenmiş dosyaları (wwwroot). Development'ta arayüz ng serve ile ayrı çalışıyor.

app.UseRouting();
app.UseCors();
app.UseRateLimiter();           // endpoint'e özel policy'ler için UseRouting'den SONRA olmalı
app.UseAuthentication();        // önce "sen kimsin"
app.UseAuthorization();         // sonra "yetkin var mı"
app.UseOutputCache();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();           // /openapi/v1.json
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "PortfolioPro API"));
}

app.MapHealthChecks("/health").DisableRateLimiting();
// Uyku moduna girmesin diye dışarıdan düzenli çağrılan hafif endpoint. Veritabanına gitmiyor,
// böylece veritabanı (Neon) boşta kalınca kendi uykusuna geçebiliyor.
app.MapGet("/ping", () => Results.Ok("pong")).DisableRateLimiting().ExcludeFromDescription();

app.MapPublicEndpoints();
app.MapAuthEndpoints();
app.MapAdminEndpoints();
app.MapStoredFiles();           // /uploads/... yüklenen dosyalar veritabanından

// Bilinmeyen /api adresleri 404 dönsün, Angular sayfası dönmesin
app.Map("/api/{**rest}", () => Results.NotFound()).ExcludeFromDescription();
// Geri kalan tüm adresler (ör. /projects/devlog) Angular'ın index.html'ine düşer, yönlendirmeyi Angular router yapar
app.MapFallbackToFile("index.html");

app.Run();

// NOT: İleride integration test yazarken WebApplicationFactory<Program> bu sınıfa erişebilsin diye
public partial class Program { }
