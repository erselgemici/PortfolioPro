using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PortfolioPro.Application.Common.Abstractions;
using PortfolioPro.Infrastructure.Identity;
using PortfolioPro.Infrastructure.Persistence;
using PortfolioPro.Infrastructure.Persistence.Interceptors;
using PortfolioPro.Infrastructure.Storage;

namespace PortfolioPro.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);

        AddPersistence(services, configuration);
        AddIdentityAndJwt(services, configuration);

        services.AddScoped<IFileStorage, DatabaseFileStorage>();

        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>("database");

        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default ayarlanmamış.");

        services.AddScoped<AuditInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            // NOT: Veritabanı PostgreSQL. SQL Server ARM işlemcili sunucularda çalışmadığı ve daha fazla RAM istediği için değiştirdim.
            options.UseNpgsql(connectionString, sql =>
            {
                // NOT: Bulutta veritabanı bağlantısı anlık kopabiliyor. Bu ayar geçici hatalarda
                // sorguyu otomatik olarak birkaç kez tekrar deniyor.
                sql.EnableRetryOnFailure(maxRetryCount: 5);
            });
            options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
        });

        // Application katmanı IAppDbContext istediğinde aynı AppDbContext örneği veriliyor
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
    }

    private static void AddIdentityAndJwt(IServiceCollection services, IConfiguration configuration)
    {
        // NOT: AddIdentity yerine AddIdentityCore. AddIdentity cookie tabanlı giriş servislerini de ekliyor,
        // biz JWT kullandığımız için sadece çekirdek kısmı (UserManager, şifre hash) yeterli.
        services.AddIdentityCore<AppUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = 10;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart(); // Key eksikse uygulama hiç ayağa kalkmasın, ilk istekte değil açılışta patlasın

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        // NOT: JwtBearer ayarlarını burada IOptions<JwtOptions> üzerinden veriyoruz.
        // Direkt configuration'dan okusaydık, "dotnet ef" komutları çalışırken Key boş olduğu için hata alırdık.
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((options, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;

                // Token'daki "sub", "role" gibi claim isimlerini Microsoft'un uzun isimlerine çevirmesin
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30), // varsayılan 5 dk, kısa ömürlü token için fazla
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };
            });

        services.AddScoped<IAuthService, AuthService>();
    }
}
