using MediatR;
using Microsoft.EntityFrameworkCore;
using PortfolioPro.Application.Common;
using PortfolioPro.Application.Common.Abstractions;

namespace PortfolioPro.Application.Features.Projects;

public static class ProjectErrors
{
    public static readonly Error NotFound = Error.NotFound("project.not_found", "Proje bulunamadı.");
    public static readonly Error SlugTaken = Error.Conflict("project.slug_taken", "Bu slug başka bir projede kullanılıyor.");
}

// ---------- Public: yayındaki projeler ----------
public sealed record GetPublishedProjectsQuery : IRequest<List<ProjectListItemDto>>;

internal sealed class GetPublishedProjectsQueryHandler(IAppDbContext db)
    : IRequestHandler<GetPublishedProjectsQuery, List<ProjectListItemDto>>
{
    public Task<List<ProjectListItemDto>> Handle(GetPublishedProjectsQuery request, CancellationToken ct) =>
        db.Projects
            .AsNoTracking()
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.ProjectDate) // en yeni proje en üstte
            .ThenBy(p => p.DisplayOrder)
            .Select(ProjectListItemDto.Projection)
            .ToListAsync(ct);
}

// ---------- Public: slug ile detay ----------
public sealed record GetProjectBySlugQuery(string Slug) : IRequest<Result<ProjectDetailDto>>;

internal sealed class GetProjectBySlugQueryHandler(IAppDbContext db)
    : IRequestHandler<GetProjectBySlugQuery, Result<ProjectDetailDto>>
{
    public async Task<Result<ProjectDetailDto>> Handle(GetProjectBySlugQuery request, CancellationToken ct)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();

        var project = await db.Projects
            .AsNoTracking()
            .Where(p => p.IsPublished && p.Slug == slug) // yayında olmayan proje dışarıdan görünmesin
            .Select(ProjectDetailDto.Projection)
            .FirstOrDefaultAsync(ct);

        return project is null ? ProjectErrors.NotFound : project;
    }
}

// ---------- Admin: tüm projeler ----------
public sealed record GetAdminProjectsQuery : IRequest<List<ProjectAdminDto>>;

internal sealed class GetAdminProjectsQueryHandler(IAppDbContext db)
    : IRequestHandler<GetAdminProjectsQuery, List<ProjectAdminDto>>
{
    public Task<List<ProjectAdminDto>> Handle(GetAdminProjectsQuery request, CancellationToken ct) =>
        db.Projects
            .AsNoTracking()
            .OrderByDescending(p => p.ProjectDate)
            .ThenBy(p => p.DisplayOrder)
            .Select(ProjectAdminDto.Projection)
            .ToListAsync(ct);
}

public sealed record GetAdminProjectByIdQuery(Guid Id) : IRequest<Result<ProjectAdminDto>>;

internal sealed class GetAdminProjectByIdQueryHandler(IAppDbContext db)
    : IRequestHandler<GetAdminProjectByIdQuery, Result<ProjectAdminDto>>
{
    public async Task<Result<ProjectAdminDto>> Handle(GetAdminProjectByIdQuery request, CancellationToken ct)
    {
        var project = await db.Projects
            .AsNoTracking()
            .Where(p => p.Id == request.Id)
            .Select(ProjectAdminDto.Projection)
            .FirstOrDefaultAsync(ct);

        return project is null ? ProjectErrors.NotFound : project;
    }
}
