using MediatR;
using Microsoft.EntityFrameworkCore;
using PortfolioPro.Application.Common.Abstractions;

namespace PortfolioPro.Application.Features.Profile;

public sealed record GetProfileQuery : IRequest<ProfileDto>;

internal sealed class GetProfileQueryHandler(IAppDbContext db) : IRequestHandler<GetProfileQuery, ProfileDto>
{
    public async Task<ProfileDto> Handle(GetProfileQuery request, CancellationToken ct)
    {
        // Profil henüz yoksa admin panelinde boş form açılsın diye Empty dönüyoruz
        var profile = await db.Profiles
            .AsNoTracking()
            .Select(ProfileDto.Projection)
            .FirstOrDefaultAsync(ct);

        return profile ?? ProfileDto.Empty;
    }
}
