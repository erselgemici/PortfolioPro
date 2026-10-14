using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using PortfolioPro.API.Extensions;
using PortfolioPro.Application.Common;
using PortfolioPro.Application.Features.Certificates;
using PortfolioPro.Application.Features.Dashboard;
using PortfolioPro.Application.Features.Educations;
using PortfolioPro.Application.Features.Experiences;
using PortfolioPro.Application.Features.Messages;
using PortfolioPro.Application.Features.Profile;
using PortfolioPro.Application.Features.Projects;
using PortfolioPro.Application.Features.Skills;
using PortfolioPro.Application.Features.Uploads;

namespace PortfolioPro.API.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var admin = app.MapGroup("/api/admin")
            .WithTags("Admin")
            .RequireAuthorization(ApiServiceExtensions.AdminPolicy) // gruptaki HER endpoint için geçerli
            .AddEndpointFilter(EvictPublicCacheOnWrite);

        admin.MapGet("/dashboard", async (ISender s, CancellationToken ct) =>
            Results.Ok(await s.Send(new GetDashboardQuery(), ct)));

        // ---------- Profil ----------
        admin.MapGet("/profile", async (ISender s, CancellationToken ct) =>
            Results.Ok(await s.Send(new GetProfileQuery(), ct)));

        admin.MapPut("/profile", async (UpdateProfileCommand cmd, ISender s, CancellationToken ct) =>
        {
            await s.Send(cmd, ct);
            return Results.NoContent();
        });

        // ---------- Projeler ----------
        var projects = admin.MapGroup("/projects");
        projects.MapGet("", async (ISender s, CancellationToken ct) => Results.Ok(await s.Send(new GetAdminProjectsQuery(), ct)));
        projects.MapGet("/{id:guid}", async (Guid id, ISender s, CancellationToken ct) => (await s.Send(new GetAdminProjectByIdQuery(id), ct)).ToHttpResult());
        projects.MapPost("", async (SaveProjectCommand cmd, ISender s, CancellationToken ct) => Created("projects", await s.Send(cmd with { Id = null }, ct)));
        projects.MapPut("/{id:guid}", async (Guid id, SaveProjectCommand cmd, ISender s, CancellationToken ct) => (await s.Send(cmd with { Id = id }, ct)).ToHttpResult());
        projects.MapDelete("/{id:guid}", async (Guid id, ISender s, CancellationToken ct) => (await s.Send(new DeleteProjectCommand(id), ct)).ToHttpResult());

        // ---------- Deneyimler ----------
        var experiences = admin.MapGroup("/experiences");
        experiences.MapGet("", async (ISender s, CancellationToken ct) => Results.Ok(await s.Send(new GetExperiencesQuery(), ct)));
        experiences.MapPost("", async (SaveExperienceCommand cmd, ISender s, CancellationToken ct) => Created("experiences", await s.Send(cmd with { Id = null }, ct)));
        experiences.MapPut("/{id:guid}", async (Guid id, SaveExperienceCommand cmd, ISender s, CancellationToken ct) => (await s.Send(cmd with { Id = id }, ct)).ToHttpResult());
        experiences.MapDelete("/{id:guid}", async (Guid id, ISender s, CancellationToken ct) => (await s.Send(new DeleteExperienceCommand(id), ct)).ToHttpResult());

        // ---------- Eğitim ----------
        var educations = admin.MapGroup("/educations");
        educations.MapGet("", async (ISender s, CancellationToken ct) => Results.Ok(await s.Send(new GetEducationsQuery(), ct)));
        educations.MapPost("", async (SaveEducationCommand cmd, ISender s, CancellationToken ct) => Created("educations", await s.Send(cmd with { Id = null }, ct)));
        educations.MapPut("/{id:guid}", async (Guid id, SaveEducationCommand cmd, ISender s, CancellationToken ct) => (await s.Send(cmd with { Id = id }, ct)).ToHttpResult());
        educations.MapDelete("/{id:guid}", async (Guid id, ISender s, CancellationToken ct) => (await s.Send(new DeleteEducationCommand(id), ct)).ToHttpResult());

        // ---------- Yetenekler ----------
        var skills = admin.MapGroup("/skills");
        skills.MapGet("", async (ISender s, CancellationToken ct) => Results.Ok(await s.Send(new GetSkillsQuery(), ct)));
        skills.MapPost("", async (SaveSkillCommand cmd, ISender s, CancellationToken ct) => Created("skills", await s.Send(cmd with { Id = null }, ct)));
        skills.MapPut("/{id:guid}", async (Guid id, SaveSkillCommand cmd, ISender s, CancellationToken ct) => (await s.Send(cmd with { Id = id }, ct)).ToHttpResult());
        skills.MapDelete("/{id:guid}", async (Guid id, ISender s, CancellationToken ct) => (await s.Send(new DeleteSkillCommand(id), ct)).ToHttpResult());

        // ---------- Sertifikalar ----------
        var certificates = admin.MapGroup("/certificates");
        certificates.MapGet("", async (ISender s, CancellationToken ct) => Results.Ok(await s.Send(new GetCertificatesQuery(), ct)));
        certificates.MapPost("", async (SaveCertificateCommand cmd, ISender s, CancellationToken ct) => Created("certificates", await s.Send(cmd with { Id = null }, ct)));
        certificates.MapPut("/{id:guid}", async (Guid id, SaveCertificateCommand cmd, ISender s, CancellationToken ct) => (await s.Send(cmd with { Id = id }, ct)).ToHttpResult());
        certificates.MapDelete("/{id:guid}", async (Guid id, ISender s, CancellationToken ct) => (await s.Send(new DeleteCertificateCommand(id), ct)).ToHttpResult());

        // ---------- Mesajlar ----------
        var messages = admin.MapGroup("/messages");
        messages.MapGet("", async (bool? unread, ISender s, CancellationToken ct) => Results.Ok(await s.Send(new GetMessagesQuery(unread), ct)));
        messages.MapPatch("/{id:guid}/read", async (Guid id, [FromQuery] bool? value, ISender s, CancellationToken ct) =>
            (await s.Send(new SetMessageReadCommand(id, value ?? true), ct)).ToHttpResult());
        messages.MapDelete("/{id:guid}", async (Guid id, ISender s, CancellationToken ct) => (await s.Send(new DeleteMessageCommand(id), ct)).ToHttpResult());

        // ---------- Dosya yükleme ----------
        // NOT: IFormFile kullanan endpoint'ler .NET 8+ varsayılan olarak antiforgery token istiyor.
        // Biz cookie ile değil Authorization header'ındaki JWT ile kimlik doğruladığımız için CSRF riski yok,
        // bu yüzden kapatıyoruz.
        admin.MapPost("/uploads", async (IFormFile file, [FromQuery] UploadKind kind, ISender s, CancellationToken ct) =>
            {
                await using var stream = file.OpenReadStream();
                var result = await s.Send(new UploadFileCommand(stream, file.FileName, file.Length, kind), ct);
                return result.IsSuccess ? Results.Ok(new { url = result.Value }) : result.Error!.ToProblem();
            })
            .DisableAntiforgery()
            .WithMetadata(new RequestSizeLimitAttribute(6 * 1024 * 1024));
    }

    // Kayıt eklenince 201 Created + yeni kaydın id'si, hata varsa ProblemDetails
    private static IResult Created(string resource, Result<Guid> result) =>
        result.IsSuccess
            ? Results.Created($"/api/admin/{resource}/{result.Value}", new { id = result.Value })
            : result.Error!.ToProblem();

    // Admin bir şey eklediğinde, güncellediğinde ya da sildiğinde public önbelleği temizliyoruz.
    // NOT: Böylece ziyaretçi değişikliği 10 dk beklemeden hemen görüyor.
    private static async ValueTask<object?> EvictPublicCacheOnWrite(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var result = await next(context);

        var http = context.HttpContext;
        if (!HttpMethods.IsGet(http.Request.Method))
        {
            var cache = http.RequestServices.GetRequiredService<IOutputCacheStore>();
            await cache.EvictByTagAsync(ApiServiceExtensions.PortfolioCacheTag, http.RequestAborted);
        }

        return result;
    }
}
