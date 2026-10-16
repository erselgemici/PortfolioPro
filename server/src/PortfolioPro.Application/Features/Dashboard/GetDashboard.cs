using MediatR;
using Microsoft.EntityFrameworkCore;
using PortfolioPro.Application.Common.Abstractions;
using PortfolioPro.Application.Features.Messages;

namespace PortfolioPro.Application.Features.Dashboard;

public sealed record DashboardDto(
    int Projects,
    int PublishedProjects,
    int Skills,
    int Experiences,
    int Certificates,
    int Messages,
    int UnreadMessages,
    List<MessageDto> LatestMessages);

public sealed record GetDashboardQuery : IRequest<DashboardDto>;

internal sealed class GetDashboardQueryHandler(IAppDbContext db) : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    public async Task<DashboardDto> Handle(GetDashboardQuery request, CancellationToken ct)
    {
        // NOT: CountAsync veritabanında COUNT(*) çalıştırıyor, kayıtları belleğe çekmiyor.
        // Aynı DbContext ile aynı anda iki sorgu çalıştırılamıyor, o yüzden Task.WhenAll yerine sorguları sırayla bekliyoruz.
        var projects = await db.Projects.CountAsync(ct);
        var published = await db.Projects.CountAsync(p => p.IsPublished, ct);
        var skills = await db.Skills.CountAsync(ct);
        var experiences = await db.Experiences.CountAsync(ct);
        var certificates = await db.Certificates.CountAsync(ct);
        var messages = await db.ContactMessages.CountAsync(ct);
        var unread = await db.ContactMessages.CountAsync(m => !m.IsRead, ct);

        var latest = await db.ContactMessages
            .AsNoTracking()
            .OrderByDescending(m => m.CreatedAt)
            .Take(5)
            .Select(MessageDto.Projection)
            .ToListAsync(ct);

        return new DashboardDto(projects, published, skills, experiences, certificates, messages, unread, latest);
    }
}
