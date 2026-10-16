using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PortfolioPro.Application.Common.Abstractions;
using PortfolioPro.Application.Common.Models;
using PortfolioPro.Application.Common.Validation;
using PortfolioPro.Domain.Entities;

namespace PortfolioPro.Application.Features.Profile;

public sealed record UpdateProfileCommand(
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
    bool IsOpenToWork) : IRequest;

public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Title).NotNull().SetValidator(new LocalizedDtoValidator(100));
        RuleFor(x => x.Headline).NotNull().SetValidator(new LocalizedDtoValidator(300));
        RuleFor(x => x.About).NotNull().SetValidator(new LocalizedDtoValidator(4000));
        RuleFor(x => x.Location).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(150);
        RuleFor(x => x.AvatarUrl).HttpUrlOrUpload();
        RuleFor(x => x.CvUrlTr).HttpUrlOrUpload();
        RuleFor(x => x.CvUrlEn).HttpUrlOrUpload();
        RuleFor(x => x.GithubUrl).HttpUrl();
        RuleFor(x => x.LinkedinUrl).HttpUrl();
        RuleFor(x => x.YearsOfExperience).InclusiveBetween(0, 60);
    }
}

internal sealed class UpdateProfileCommandHandler(IAppDbContext db) : IRequestHandler<UpdateProfileCommand>
{
    public async Task Handle(UpdateProfileCommand request, CancellationToken ct)
    {
        // Kayıt yoksa oluştur, varsa güncelle (upsert). Bu tabloda hep tek satır olacak.
        var profile = await db.Profiles.FirstOrDefaultAsync(ct);
        if (profile is null)
        {
            profile = new SiteProfile();
            db.Profiles.Add(profile);
        }

        profile.FullName = request.FullName.Trim();
        profile.Title = request.Title.ToEntity();
        profile.Headline = request.Headline.ToEntity();
        profile.About = request.About.ToEntity();
        profile.Location = request.Location.Trim();
        profile.Email = request.Email.Trim();
        profile.AvatarUrl = request.AvatarUrl;
        profile.CvUrlTr = request.CvUrlTr;
        profile.CvUrlEn = request.CvUrlEn;
        profile.GithubUrl = request.GithubUrl;
        profile.LinkedinUrl = request.LinkedinUrl;
        profile.YearsOfExperience = request.YearsOfExperience;
        profile.IsOpenToWork = request.IsOpenToWork;

        await db.SaveChangesAsync(ct);
    }
}
