using MediatR;
using PortfolioPro.API.Extensions;
using PortfolioPro.Application.Features.Messages;
using PortfolioPro.Application.Features.Portfolio;
using PortfolioPro.Application.Features.Projects;

namespace PortfolioPro.API.Endpoints;

public static class PublicEndpoints
{
    public static void MapPublicEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api").WithTags("Public");

        // Ana sayfanın tüm verisi tek istekte, OutputCache ile önbellekli
        api.MapGet("/portfolio", async (ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetPortfolioQuery(), ct)))
            .CacheOutput(ApiServiceExtensions.PublicCachePolicy);

        api.MapGet("/projects", async (ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetPublishedProjectsQuery(), ct)))
            .CacheOutput(ApiServiceExtensions.PublicCachePolicy);

        api.MapGet("/projects/{slug}", async (string slug, ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetProjectBySlugQuery(slug), ct)).ToHttpResult())
            .CacheOutput(ApiServiceExtensions.PublicCachePolicy);

        api.MapPost("/contact", async (SendContactMessageCommand command, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(command, ct);
                return Results.Accepted();
            })
            .RequireRateLimiting(ApiServiceExtensions.ContactRateLimit);
    }
}
