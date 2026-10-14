using System.Security.Claims;
using MediatR;
using PortfolioPro.API.Extensions;
using PortfolioPro.Application.Common.Models;
using PortfolioPro.Application.Features.Auth;

namespace PortfolioPro.API.Endpoints;

public static class AuthEndpoints
{
    // NOT: Refresh token JavaScript'in okuyamadığı httpOnly cookie'de duruyor.
    // Access token ise Angular'da sadece bellekte (localStorage'da DEĞİL). Sayfada bir XSS açığı olsa bile
    // saldırgan uzun ömürlü refresh token'a erişemiyor.
    private const string RefreshCookieName = "pp_refresh";
    private const string RefreshCookiePath = "/api/auth";

    public sealed record AccessTokenResponse(string AccessToken, DateTime ExpiresAt, AdminUserDto User);
    public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/auth").WithTags("Auth");

        auth.MapPost("/login", async (LoginCommand command, ISender sender, HttpContext http, CancellationToken ct) =>
            {
                var result = await sender.Send(command, ct);
                if (result.IsFailure)
                    return result.Error!.ToProblem();

                return SignIn(http, result.Value);
            })
            .RequireRateLimiting(ApiServiceExtensions.AuthRateLimit);

        auth.MapPost("/refresh", async (ISender sender, HttpContext http, CancellationToken ct) =>
            {
                var token = http.Request.Cookies[RefreshCookieName];
                if (string.IsNullOrEmpty(token))
                    return Results.Unauthorized();

                var result = await sender.Send(new RefreshTokenCommand(token), ct);
                if (result.IsFailure)
                {
                    DeleteRefreshCookie(http);
                    return result.Error!.ToProblem();
                }

                return SignIn(http, result.Value);
            });
        // NOT: Refresh'e login gibi sıkı limit koymadım. Sayfa her yenilendiğinde refresh çağrılıyor,
        // 64 byte'lık rastgele token tahmin edilemeyeceği için genel limit (dakikada 120) yeterli.

        auth.MapPost("/logout", async (ISender sender, HttpContext http, CancellationToken ct) =>
        {
            await sender.Send(new LogoutCommand(http.Request.Cookies[RefreshCookieName]), ct);
            DeleteRefreshCookie(http);
            return Results.NoContent();
        });

        auth.MapGet("/me", (ClaimsPrincipal user) => Results.Ok(new
            {
                Id = user.FindFirstValue("sub"),
                Email = user.FindFirstValue("email"),
                FullName = user.FindFirstValue("name")
            }))
            .RequireAuthorization(ApiServiceExtensions.AdminPolicy);

        auth.MapPost("/change-password", async (ChangePasswordRequest request, ClaimsPrincipal user, ISender sender, HttpContext http, CancellationToken ct) =>
            {
                if (!Guid.TryParse(user.FindFirstValue("sub"), out var userId))
                    return Results.Unauthorized();

                var result = await sender.Send(new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword), ct);
                if (result.IsSuccess)
                    DeleteRefreshCookie(http); // tüm oturumlar kapandı, tekrar giriş gerekecek

                return result.ToHttpResult();
            })
            .RequireAuthorization(ApiServiceExtensions.AdminPolicy);
    }

    private static IResult SignIn(HttpContext http, AuthTokens tokens)
    {
        http.Response.Cookies.Append(RefreshCookieName, tokens.RefreshToken, new CookieOptions
        {
            HttpOnly = true,                 // JavaScript okuyamaz
            Secure = true,                   // sadece https (localhost tarayıcılar tarafından istisna)
            SameSite = SameSiteMode.Strict,  // başka siteden gelen isteklerde gönderilmez (CSRF koruması)
            Path = RefreshCookiePath,        // sadece /api/auth isteklerinde gönderilir
            Expires = tokens.RefreshTokenExpiresAt
        });

        return Results.Ok(new AccessTokenResponse(tokens.AccessToken, tokens.AccessTokenExpiresAt, tokens.User));
    }

    private static void DeleteRefreshCookie(HttpContext http) =>
        http.Response.Cookies.Delete(RefreshCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = RefreshCookiePath
        });
}
