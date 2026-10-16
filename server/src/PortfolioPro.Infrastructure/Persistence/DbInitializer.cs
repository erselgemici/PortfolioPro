using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PortfolioPro.Infrastructure.Identity;
using PortfolioPro.Infrastructure.Persistence.Seed;

namespace PortfolioPro.Infrastructure.Persistence;

public static class DbInitializer
{
    public const string AdminRole = "Admin";

    public static async Task InitializeDatabaseAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;

        var configuration = services.GetRequiredService<IConfiguration>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DbInitializer));
        var db = services.GetRequiredService<AppDbContext>();

        // Bekleyen migration varsa uygula. Büyük projelerde bu iş genelde deploy sırasında ayrıca yapılıyor.
        // Migration hiç oluşturulmadıysa tablolar oluşmaz ve aşağıda anlaşılması zor bir SQL hatası alınır.
        // Bunun yerine ne yapılması gerektiğini söyleyen açık bir hata veriyoruz.
        if (!db.Database.GetMigrations().Any())
            throw new InvalidOperationException(
                "Hiç migration bulunamadı. Önce şu komutu çalıştırın (server klasöründe): " +
                "dotnet ef migrations add InitialCreate --project src/PortfolioPro.Infrastructure " +
                "--startup-project src/PortfolioPro.API --output-dir Persistence/Migrations");

        if (configuration.GetValue("Database:MigrateOnStartup", true))
        {
            logger.LogInformation("Migration'lar uygulanıyor...");
            await db.Database.MigrateAsync();
        }

        await SeedAdminAsync(services, configuration, logger);

        if (configuration.GetValue("Database:SeedSampleContent", true))
            await SeedData.SeedContentAsync(db, logger);
    }

    private static async Task SeedAdminAsync(IServiceProvider services, IConfiguration configuration, ILogger logger)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();

        if (!await roleManager.RoleExistsAsync(AdminRole))
            await roleManager.CreateAsync(new IdentityRole<Guid>(AdminRole));

        var admins = await userManager.GetUsersInRoleAsync(AdminRole);
        if (admins.Count > 0)
            return;

        // NOT: Admin bilgileri koda yazılmıyor. user-secrets ya da ortam değişkeninden geliyor:
        // dotnet user-secrets set "Seed:AdminEmail" "..."  /  Seed__AdminPassword=...
        var email = configuration["Seed:AdminEmail"];
        var password = configuration["Seed:AdminPassword"];
        var fullName = configuration["Seed:AdminFullName"] ?? "Admin";

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("Admin kullanıcısı yok ve Seed:AdminEmail / Seed:AdminPassword ayarlanmamış. Admin paneline giriş yapılamaz.");
            return;
        }

        var admin = new AppUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = fullName
        };

        var result = await userManager.CreateAsync(admin, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogError("Admin oluşturulamadı: {Errors}", errors);
            return;
        }

        await userManager.AddToRoleAsync(admin, AdminRole);
        logger.LogInformation("Admin kullanıcısı oluşturuldu: {Email}", email);
    }
}
