using PortfolioPro.API.Endpoints;
using PortfolioPro.API.Extensions;
using PortfolioPro.API.Middleware;
using PortfolioPro.Application;
using PortfolioPro.Infrastructure;
using PortfolioPro.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

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

app.UseForwardedHeaders();      // nginx arkasında gerçek IP ve https bilgisini al (rate limit IP'ye göre çalışıyor)
app.UseExceptionHandler();      // yakalanmayan hataları ProblemDetails JSON'a çevir

if (!app.Environment.IsDevelopment())
    app.UseHsts();

app.UseSecurityHeaders();
app.UseResponseCompression();
app.UseUploadsStaticFiles();    // /uploads altındaki yüklenen görseller

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

app.MapPublicEndpoints();
app.MapAuthEndpoints();
app.MapAdminEndpoints();

app.Run();

// NOT: İleride integration test yazarken WebApplicationFactory<Program> bu sınıfa erişebilsin diye
public partial class Program { }
