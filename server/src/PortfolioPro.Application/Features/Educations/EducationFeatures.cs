using System.Linq.Expressions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PortfolioPro.Application.Common;
using PortfolioPro.Application.Common.Abstractions;
using PortfolioPro.Application.Common.Models;
using PortfolioPro.Application.Common.Validation;
using PortfolioPro.Domain.Entities;

namespace PortfolioPro.Application.Features.Educations;

public sealed record EducationDto(
    Guid Id,
    string School,
    LocalizedDto Department,
    LocalizedDto Degree,
    LocalizedDto Description,
    int StartYear,
    int? EndYear,
    int DisplayOrder)
{
    public static readonly Expression<Func<Education, EducationDto>> Projection = e => new EducationDto(
        e.Id,
        e.School,
        new LocalizedDto(e.Department.Tr, e.Department.En),
        new LocalizedDto(e.Degree.Tr, e.Degree.En),
        new LocalizedDto(e.Description.Tr, e.Description.En),
        e.StartYear,
        e.EndYear,
        e.DisplayOrder);
}

public sealed record GetEducationsQuery : IRequest<List<EducationDto>>;

internal sealed class GetEducationsQueryHandler(IAppDbContext db)
    : IRequestHandler<GetEducationsQuery, List<EducationDto>>
{
    public Task<List<EducationDto>> Handle(GetEducationsQuery request, CancellationToken ct) =>
        db.Educations
            .AsNoTracking()
            .OrderBy(e => e.DisplayOrder)
            .ThenByDescending(e => e.StartYear)
            .Select(EducationDto.Projection)
            .ToListAsync(ct);
}

public sealed record SaveEducationCommand(
    Guid? Id,
    string School,
    LocalizedDto Department,
    LocalizedDto Degree,
    LocalizedDto Description,
    int StartYear,
    int? EndYear,
    int DisplayOrder) : IRequest<Result<Guid>>;

public sealed class SaveEducationCommandValidator : AbstractValidator<SaveEducationCommand>
{
    public SaveEducationCommandValidator()
    {
        RuleFor(x => x.School).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Department).NotNull().SetValidator(new LocalizedDtoValidator(150));
        RuleFor(x => x.Degree).NotNull().SetValidator(new LocalizedDtoValidator(100));
        RuleFor(x => x.Description).NotNull().SetValidator(new LocalizedDtoValidator(1000, required: false));
        RuleFor(x => x.StartYear).InclusiveBetween(1980, 2100);
        RuleFor(x => x.EndYear)
            .GreaterThanOrEqualTo(x => x.StartYear)
            .When(x => x.EndYear.HasValue)
            .WithMessage("Bitiş yılı başlangıçtan önce olamaz.");
    }
}

internal sealed class SaveEducationCommandHandler(IAppDbContext db)
    : IRequestHandler<SaveEducationCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(SaveEducationCommand request, CancellationToken ct)
    {
        Education? entity;
        if (request.Id is { } id)
        {
            entity = await db.Educations.FirstOrDefaultAsync(e => e.Id == id, ct);
            if (entity is null)
                return Error.NotFound("education.not_found", "Eğitim kaydı bulunamadı.");
        }
        else
        {
            entity = new Education();
            db.Educations.Add(entity);
        }

        entity.School = request.School.Trim();
        entity.Department = request.Department.ToEntity();
        entity.Degree = request.Degree.ToEntity();
        entity.Description = request.Description.ToEntity();
        entity.StartYear = request.StartYear;
        entity.EndYear = request.EndYear;
        entity.DisplayOrder = request.DisplayOrder;

        await db.SaveChangesAsync(ct);
        return entity.Id;
    }
}

public sealed record DeleteEducationCommand(Guid Id) : IRequest<Result>;

internal sealed class DeleteEducationCommandHandler(IAppDbContext db)
    : IRequestHandler<DeleteEducationCommand, Result>
{
    public async Task<Result> Handle(DeleteEducationCommand request, CancellationToken ct)
    {
        var deleted = await db.Educations.Where(e => e.Id == request.Id).ExecuteDeleteAsync(ct);
        return deleted == 0
            ? Error.NotFound("education.not_found", "Eğitim kaydı bulunamadı.")
            : Result.Success();
    }
}
