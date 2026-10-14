using MediatR;
using Microsoft.EntityFrameworkCore;
using PortfolioPro.Application.Common.Abstractions;
using PortfolioPro.Application.Features.Certificates;
using PortfolioPro.Application.Features.Educations;
using PortfolioPro.Application.Features.Experiences;
using PortfolioPro.Application.Features.Profile;
using PortfolioPro.Application.Features.Projects;
using PortfolioPro.Application.Features.Skills;

namespace PortfolioPro.Application.Features.Portfolio;

// Ana sayfanın ihtiyacı olan her şey tek istekte.
// NOT: Angular tarafı 6 ayrı istek atmak yerine tek istek atıyor. API tarafında da bu cevap
// OutputCache ile önbelleğe alınıyor, admin bir şey değiştirene kadar veritabanına hiç gidilmiyor.
public sealed record PortfolioDto(
    ProfileDto? Profile,
    List<SkillDto> Skills,
    List<ExperienceDto> Experiences,
    List<EducationDto> Educations,
    List<CertificateDto> Certificates,
    List<ProjectListItemDto> FeaturedProjects);

public sealed record GetPortfolioQuery : IRequest<PortfolioDto>;

internal sealed class GetPortfolioQueryHandler(IAppDbContext db, ISender sender)
    : IRequestHandler<GetPortfolioQuery, PortfolioDto>
{
    public async Task<PortfolioDto> Handle(GetPortfolioQuery request, CancellationToken ct)
    {
        var profile = await db.Profiles
            .AsNoTracking()
            .Select(ProfileDto.Projection)
            .FirstOrDefaultAsync(ct);

        // Liste sorguları zaten var, aynı kodu tekrar yazmak yerine onları çağırıyorum
        var skills = await sender.Send(new GetSkillsQuery(), ct);
        var experiences = await sender.Send(new GetExperiencesQuery(), ct);
        var educations = await sender.Send(new GetEducationsQuery(), ct);
        var certificates = await sender.Send(new GetCertificatesQuery(), ct);

        var featured = await db.Projects
            .AsNoTracking()
            .Where(p => p.IsPublished && p.IsFeatured)
            .OrderByDescending(p => p.ProjectDate)
            .ThenBy(p => p.DisplayOrder)
            .Take(6)
            .Select(ProjectListItemDto.Projection)
            .ToListAsync(ct);

        return new PortfolioDto(profile, skills, experiences, educations, certificates, featured);
    }
}
