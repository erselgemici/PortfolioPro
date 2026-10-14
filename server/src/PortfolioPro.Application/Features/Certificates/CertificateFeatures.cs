using System.Linq.Expressions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PortfolioPro.Application.Common;
using PortfolioPro.Application.Common.Abstractions;
using PortfolioPro.Application.Common.Validation;
using PortfolioPro.Domain.Entities;

namespace PortfolioPro.Application.Features.Certificates;

public sealed record CertificateDto(Guid Id, string Title, string Issuer, DateOnly IssuedAt, string? CredentialUrl, int DisplayOrder)
{
    public static readonly Expression<Func<Certificate, CertificateDto>> Projection =
        c => new CertificateDto(c.Id, c.Title, c.Issuer, c.IssuedAt, c.CredentialUrl, c.DisplayOrder);
}

public sealed record GetCertificatesQuery : IRequest<List<CertificateDto>>;

internal sealed class GetCertificatesQueryHandler(IAppDbContext db)
    : IRequestHandler<GetCertificatesQuery, List<CertificateDto>>
{
    public Task<List<CertificateDto>> Handle(GetCertificatesQuery request, CancellationToken ct) =>
        db.Certificates
            .AsNoTracking()
            .OrderBy(c => c.DisplayOrder)
            .ThenByDescending(c => c.IssuedAt)
            .Select(CertificateDto.Projection)
            .ToListAsync(ct);
}

public sealed record SaveCertificateCommand(
    Guid? Id,
    string Title,
    string Issuer,
    DateOnly IssuedAt,
    string? CredentialUrl,
    int DisplayOrder) : IRequest<Result<Guid>>;

public sealed class SaveCertificateCommandValidator : AbstractValidator<SaveCertificateCommand>
{
    public SaveCertificateCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Issuer).NotEmpty().MaximumLength(100);
        RuleFor(x => x.CredentialUrl).HttpUrl();
    }
}

internal sealed class SaveCertificateCommandHandler(IAppDbContext db)
    : IRequestHandler<SaveCertificateCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(SaveCertificateCommand request, CancellationToken ct)
    {
        Certificate? entity;
        if (request.Id is { } id)
        {
            entity = await db.Certificates.FirstOrDefaultAsync(c => c.Id == id, ct);
            if (entity is null)
                return Error.NotFound("certificate.not_found", "Sertifika bulunamadı.");
        }
        else
        {
            entity = new Certificate();
            db.Certificates.Add(entity);
        }

        entity.Title = request.Title.Trim();
        entity.Issuer = request.Issuer.Trim();
        entity.IssuedAt = request.IssuedAt;
        entity.CredentialUrl = request.CredentialUrl;
        entity.DisplayOrder = request.DisplayOrder;

        await db.SaveChangesAsync(ct);
        return entity.Id;
    }
}

public sealed record DeleteCertificateCommand(Guid Id) : IRequest<Result>;

internal sealed class DeleteCertificateCommandHandler(IAppDbContext db)
    : IRequestHandler<DeleteCertificateCommand, Result>
{
    public async Task<Result> Handle(DeleteCertificateCommand request, CancellationToken ct)
    {
        var deleted = await db.Certificates.Where(c => c.Id == request.Id).ExecuteDeleteAsync(ct);
        return deleted == 0
            ? Error.NotFound("certificate.not_found", "Sertifika bulunamadı.")
            : Result.Success();
    }
}
