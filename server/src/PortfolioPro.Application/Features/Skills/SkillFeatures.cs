using System.Linq.Expressions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PortfolioPro.Application.Common;
using PortfolioPro.Application.Common.Abstractions;
using PortfolioPro.Domain.Entities;
using PortfolioPro.Domain.Enums;

namespace PortfolioPro.Application.Features.Skills;

public sealed record SkillDto(Guid Id, string Name, SkillCategory Category, bool IsHighlighted, int DisplayOrder)
{
    public static readonly Expression<Func<Skill, SkillDto>> Projection =
        s => new SkillDto(s.Id, s.Name, s.Category, s.IsHighlighted, s.DisplayOrder);
}

public sealed record GetSkillsQuery : IRequest<List<SkillDto>>;

internal sealed class GetSkillsQueryHandler(IAppDbContext db) : IRequestHandler<GetSkillsQuery, List<SkillDto>>
{
    public Task<List<SkillDto>> Handle(GetSkillsQuery request, CancellationToken ct) =>
        db.Skills
            .AsNoTracking()
            .OrderBy(s => s.Category)
            .ThenBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .Select(SkillDto.Projection)
            .ToListAsync(ct);
}

public sealed record SaveSkillCommand(Guid? Id, string Name, SkillCategory Category, bool IsHighlighted, int DisplayOrder)
    : IRequest<Result<Guid>>;

public sealed class SaveSkillCommandValidator : AbstractValidator<SaveSkillCommand>
{
    public SaveSkillCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        // NOT: Enum'a JSON'dan olmayan bir sayı da gelebilir (örn. 99), IsInEnum bunu engelliyor
        RuleFor(x => x.Category).IsInEnum();
    }
}

internal sealed class SaveSkillCommandHandler(IAppDbContext db) : IRequestHandler<SaveSkillCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(SaveSkillCommand request, CancellationToken ct)
    {
        var name = request.Name.Trim();

        // Aynı isimde iki yetenek olmasın
        var duplicate = await db.Skills.AnyAsync(s => s.Name == name && s.Id != request.Id, ct);
        if (duplicate)
            return Error.Conflict("skill.duplicate", "Bu yetenek zaten ekli.");

        Skill? entity;
        if (request.Id is { } id)
        {
            entity = await db.Skills.FirstOrDefaultAsync(s => s.Id == id, ct);
            if (entity is null)
                return Error.NotFound("skill.not_found", "Yetenek bulunamadı.");
        }
        else
        {
            entity = new Skill();
            db.Skills.Add(entity);
        }

        entity.Name = name;
        entity.Category = request.Category;
        entity.IsHighlighted = request.IsHighlighted;
        entity.DisplayOrder = request.DisplayOrder;

        await db.SaveChangesAsync(ct);
        return entity.Id;
    }
}

public sealed record DeleteSkillCommand(Guid Id) : IRequest<Result>;

internal sealed class DeleteSkillCommandHandler(IAppDbContext db) : IRequestHandler<DeleteSkillCommand, Result>
{
    public async Task<Result> Handle(DeleteSkillCommand request, CancellationToken ct)
    {
        var deleted = await db.Skills.Where(s => s.Id == request.Id).ExecuteDeleteAsync(ct);
        return deleted == 0
            ? Error.NotFound("skill.not_found", "Yetenek bulunamadı.")
            : Result.Success();
    }
}
