using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PortfolioPro.Application.Common;
using PortfolioPro.Application.Common.Abstractions;
using PortfolioPro.Application.Common.Helpers;
using PortfolioPro.Application.Common.Models;
using PortfolioPro.Application.Common.Validation;
using PortfolioPro.Domain.Entities;
using PortfolioPro.Domain.Enums;

namespace PortfolioPro.Application.Features.Projects;

public sealed record SaveProjectCommand(
    Guid? Id,
    string? Slug,
    LocalizedDto Title,
    LocalizedDto Summary,
    LocalizedDto Content,
    ProjectCategory Category,
    string? CoverImageUrl,
    string? GithubUrl,
    string? LiveUrl,
    string? StoreUrl,
    List<string>? Technologies,
    DateOnly ProjectDate,
    bool IsFeatured,
    bool IsPublished,
    int DisplayOrder) : IRequest<Result<Guid>>;

public sealed class SaveProjectCommandValidator : AbstractValidator<SaveProjectCommand>
{
    public SaveProjectCommandValidator()
    {
        RuleFor(x => x.Slug)
            .MaximumLength(80)
            .Matches("^[a-z0-9]+(-[a-z0-9]+)*$")
            .When(x => !string.IsNullOrWhiteSpace(x.Slug))
            .WithMessage("Slug sadece küçük harf, rakam ve tire içerebilir.");
        RuleFor(x => x.Title).NotNull().SetValidator(new LocalizedDtoValidator(150));
        RuleFor(x => x.Summary).NotNull().SetValidator(new LocalizedDtoValidator(400));
        RuleFor(x => x.Content).NotNull().SetValidator(new LocalizedDtoValidator(20000, required: false));
        RuleFor(x => x.Category).IsInEnum();
        RuleFor(x => x.ProjectDate)
            .GreaterThan(new DateOnly(2000, 1, 1))
            .WithMessage("Proje tarihi giriniz.");
        RuleFor(x => x.CoverImageUrl).HttpUrlOrUpload();
        RuleFor(x => x.GithubUrl).HttpUrl();
        RuleFor(x => x.LiveUrl).HttpUrl();
        RuleFor(x => x.StoreUrl).HttpUrl();
        RuleForEach(x => x.Technologies).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Technologies).Must(t => t is null || t.Count <= 20)
            .WithMessage("En fazla 20 teknoloji eklenebilir.");
    }
}

internal sealed class SaveProjectCommandHandler(IAppDbContext db) : IRequestHandler<SaveProjectCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(SaveProjectCommand request, CancellationToken ct)
    {
        // Slug boş bırakıldıysa İngilizce başlıktan üretiyoruz (yoksa Türkçe)
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugHelper.Generate(string.IsNullOrWhiteSpace(request.Title.En) ? request.Title.Tr : request.Title.En)
            : request.Slug.Trim().ToLowerInvariant();

        var slugTaken = await db.Projects.AnyAsync(p => p.Slug == slug && p.Id != request.Id, ct);
        if (slugTaken)
            return ProjectErrors.SlugTaken;

        Project? project;
        if (request.Id is { } id)
        {
            project = await db.Projects.FirstOrDefaultAsync(p => p.Id == id, ct);
            if (project is null)
                return ProjectErrors.NotFound;
        }
        else
        {
            project = new Project();
            db.Projects.Add(project);
        }

        project.Slug = slug;
        project.Title = request.Title.ToEntity();
        project.Summary = request.Summary.ToEntity();
        project.Content = request.Content.ToEntity();
        project.Category = request.Category;
        project.CoverImageUrl = request.CoverImageUrl;
        project.GithubUrl = request.GithubUrl;
        project.LiveUrl = request.LiveUrl;
        project.StoreUrl = request.StoreUrl;
        project.Technologies = request.Technologies?.Select(t => t.Trim()).Distinct().ToList() ?? [];
        project.ProjectDate = request.ProjectDate;
        project.IsFeatured = request.IsFeatured;
        project.IsPublished = request.IsPublished;
        project.DisplayOrder = request.DisplayOrder;

        await db.SaveChangesAsync(ct);
        return project.Id;
    }
}

public sealed record DeleteProjectCommand(Guid Id) : IRequest<Result>;

internal sealed class DeleteProjectCommandHandler(IAppDbContext db) : IRequestHandler<DeleteProjectCommand, Result>
{
    public async Task<Result> Handle(DeleteProjectCommand request, CancellationToken ct)
    {
        var deleted = await db.Projects.Where(p => p.Id == request.Id).ExecuteDeleteAsync(ct);
        return deleted == 0 ? ProjectErrors.NotFound : Result.Success();
    }
}
