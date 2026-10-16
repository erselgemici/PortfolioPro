using System.Linq.Expressions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PortfolioPro.Application.Common;
using PortfolioPro.Application.Common.Abstractions;
using PortfolioPro.Application.Common.Models;
using PortfolioPro.Application.Common.Validation;
using PortfolioPro.Domain.Entities;

namespace PortfolioPro.Application.Features.Experiences;

// NOT: Küçük CRUD özelliklerinde dto, query, command, validator ve handler'ı tek dosyada tuttum.
// Bir özelliğe bakarken 6 farklı dosya arasında gezmemek için.

public sealed record ExperienceDto(
    Guid Id,
    string Company,
    LocalizedDto Position,
    LocalizedDto Description,
    string? Location,
    DateOnly StartDate,
    DateOnly? EndDate,
    List<string> Technologies,
    int DisplayOrder)
{
    public static readonly Expression<Func<Experience, ExperienceDto>> Projection = e => new ExperienceDto(
        e.Id,
        e.Company,
        new LocalizedDto(e.Position.Tr, e.Position.En),
        new LocalizedDto(e.Description.Tr, e.Description.En),
        e.Location,
        e.StartDate,
        e.EndDate,
        e.Technologies,
        e.DisplayOrder);
}

// ---------- Listele ----------
public sealed record GetExperiencesQuery : IRequest<List<ExperienceDto>>;

internal sealed class GetExperiencesQueryHandler(IAppDbContext db)
    : IRequestHandler<GetExperiencesQuery, List<ExperienceDto>>
{
    public Task<List<ExperienceDto>> Handle(GetExperiencesQuery request, CancellationToken ct) =>
        db.Experiences
            .AsNoTracking()
            .OrderBy(e => e.DisplayOrder)
            .ThenByDescending(e => e.StartDate)
            .Select(ExperienceDto.Projection)
            .ToListAsync(ct);
}

// ---------- Ekle / Güncelle ----------
// Id null ise yeni kayıt, doluysa güncelleme
public sealed record SaveExperienceCommand(
    Guid? Id,
    string Company,
    LocalizedDto Position,
    LocalizedDto Description,
    string? Location,
    DateOnly StartDate,
    DateOnly? EndDate,
    List<string>? Technologies,
    int DisplayOrder) : IRequest<Result<Guid>>;

public sealed class SaveExperienceCommandValidator : AbstractValidator<SaveExperienceCommand>
{
    public SaveExperienceCommandValidator()
    {
        RuleFor(x => x.Company).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Position).NotNull().SetValidator(new LocalizedDtoValidator(150));
        RuleFor(x => x.Description).NotNull().SetValidator(new LocalizedDtoValidator(3000));
        RuleFor(x => x.Location).MaximumLength(100);
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.EndDate.HasValue)
            .WithMessage("Bitiş tarihi başlangıçtan önce olamaz.");
        RuleForEach(x => x.Technologies).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Technologies).Must(t => t is null || t.Count <= 20);
    }
}

internal sealed class SaveExperienceCommandHandler(IAppDbContext db)
    : IRequestHandler<SaveExperienceCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(SaveExperienceCommand request, CancellationToken ct)
    {
        Experience? entity;
        if (request.Id is { } id)
        {
            entity = await db.Experiences.FirstOrDefaultAsync(e => e.Id == id, ct);
            if (entity is null)
                return Error.NotFound("experience.not_found", "Deneyim bulunamadı.");
        }
        else
        {
            entity = new Experience();
            db.Experiences.Add(entity);
        }

        entity.Company = request.Company.Trim();
        entity.Position = request.Position.ToEntity();
        entity.Description = request.Description.ToEntity();
        entity.Location = request.Location?.Trim();
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.Technologies = request.Technologies?.Select(t => t.Trim()).Distinct().ToList() ?? [];
        entity.DisplayOrder = request.DisplayOrder;

        await db.SaveChangesAsync(ct);
        return entity.Id;
    }
}

// ---------- Sil ----------
public sealed record DeleteExperienceCommand(Guid Id) : IRequest<Result>;

internal sealed class DeleteExperienceCommandHandler(IAppDbContext db)
    : IRequestHandler<DeleteExperienceCommand, Result>
{
    public async Task<Result> Handle(DeleteExperienceCommand request, CancellationToken ct)
    {
        // NOT: ExecuteDeleteAsync kaydı önce veritabanından çekmeden direkt DELETE sorgusu çalıştırıyor, daha hızlı
        var deleted = await db.Experiences.Where(e => e.Id == request.Id).ExecuteDeleteAsync(ct);
        return deleted == 0
            ? Error.NotFound("experience.not_found", "Deneyim bulunamadı.")
            : Result.Success();
    }
}
