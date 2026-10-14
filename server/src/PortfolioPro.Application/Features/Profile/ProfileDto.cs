using System.Linq.Expressions;
using PortfolioPro.Application.Common.Models;
using PortfolioPro.Domain.Entities;

namespace PortfolioPro.Application.Features.Profile;

public sealed record ProfileDto(
    string FullName,
    LocalizedDto Title,
    LocalizedDto Headline,
    LocalizedDto About,
    string Location,
    string Email,
    string? AvatarUrl,
    string? CvUrlTr,
    string? CvUrlEn,
    string? GithubUrl,
    string? LinkedinUrl,
    int YearsOfExperience,
    bool IsOpenToWork)
{
    // NOT: Projeksiyonu Expression olarak yazdım. Select(ProfileDto.Projection) dediğimde EF Core bunu
    // SQL'e çeviriyor ve sadece bu kolonları çekiyor. AutoMapper kullanmadan da aynı işi görüyor.
    public static readonly Expression<Func<SiteProfile, ProfileDto>> Projection = p => new ProfileDto(
        p.FullName,
        new LocalizedDto(p.Title.Tr, p.Title.En),
        new LocalizedDto(p.Headline.Tr, p.Headline.En),
        new LocalizedDto(p.About.Tr, p.About.En),
        p.Location,
        p.Email,
        p.AvatarUrl,
        p.CvUrlTr,
        p.CvUrlEn,
        p.GithubUrl,
        p.LinkedinUrl,
        p.YearsOfExperience,
        p.IsOpenToWork);

    public static ProfileDto Empty => new(
        string.Empty, new("", ""), new("", ""), new("", ""), string.Empty, string.Empty,
        null, null, null, null, null, 0, false);
}
