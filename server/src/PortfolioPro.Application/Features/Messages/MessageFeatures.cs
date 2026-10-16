using System.Linq.Expressions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PortfolioPro.Application.Common;
using PortfolioPro.Application.Common.Abstractions;
using PortfolioPro.Domain.Entities;

namespace PortfolioPro.Application.Features.Messages;

public sealed record MessageDto(Guid Id, string Name, string Email, string? Subject, string Body, bool IsRead, DateTime CreatedAt)
{
    public static readonly Expression<Func<ContactMessage, MessageDto>> Projection =
        m => new MessageDto(m.Id, m.Name, m.Email, m.Subject, m.Body, m.IsRead, m.CreatedAt);
}

// ---------- Public: iletişim formu ----------
// Website alanı bir "honeypot". Formda CSS ile gizli duruyor, gerçek kullanıcı göremediği için boş bırakır.
// NOT: Spam botları formdaki tüm inputları doldurur. Bu alan doluysa gelen istek büyük ihtimalle bottur.
public sealed record SendContactMessageCommand(string Name, string Email, string? Subject, string Message, string? Website)
    : IRequest;

public sealed class SendContactMessageCommandValidator : AbstractValidator<SendContactMessageCommand>
{
    public SendContactMessageCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(2).MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(150);
        RuleFor(x => x.Subject).MaximumLength(150);
        RuleFor(x => x.Message).NotEmpty().MinimumLength(10).MaximumLength(4000);
    }
}

internal sealed class SendContactMessageCommandHandler(IAppDbContext db) : IRequestHandler<SendContactMessageCommand>
{
    public async Task Handle(SendContactMessageCommand request, CancellationToken ct)
    {
        // Bot ise hiçbir şey kaydetmiyoruz ama "başarılı" dönüyoruz ki bot yakalandığını anlamasın
        if (!string.IsNullOrWhiteSpace(request.Website))
            return;

        db.ContactMessages.Add(new ContactMessage
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            Subject = string.IsNullOrWhiteSpace(request.Subject) ? null : request.Subject.Trim(),
            Body = request.Message.Trim()
        });

        await db.SaveChangesAsync(ct);
    }
}

// ---------- Admin ----------
public sealed record GetMessagesQuery(bool? OnlyUnread) : IRequest<List<MessageDto>>;

internal sealed class GetMessagesQueryHandler(IAppDbContext db) : IRequestHandler<GetMessagesQuery, List<MessageDto>>
{
    public Task<List<MessageDto>> Handle(GetMessagesQuery request, CancellationToken ct)
    {
        var query = db.ContactMessages.AsNoTracking();

        if (request.OnlyUnread == true)
            query = query.Where(m => !m.IsRead);

        // Kişisel portfolyo için 500 mesaj fazlasıyla yeterli, sayfalama gerekirse burası değişir
        return query
            .OrderByDescending(m => m.CreatedAt)
            .Take(500)
            .Select(MessageDto.Projection)
            .ToListAsync(ct);
    }
}

public sealed record SetMessageReadCommand(Guid Id, bool IsRead) : IRequest<Result>;

internal sealed class SetMessageReadCommandHandler(IAppDbContext db) : IRequestHandler<SetMessageReadCommand, Result>
{
    public async Task<Result> Handle(SetMessageReadCommand request, CancellationToken ct)
    {
        // NOT: ExecuteUpdateAsync tek bir alanı, kaydı çekmeden direkt UPDATE sorgusuyla güncelliyor.
        // Dikkat: SaveChanges'i kullanmadığı için AuditInterceptor çalışmıyor, UpdatedAt kendiliğinden dolmuyor.
        var updated = await db.ContactMessages
            .Where(m => m.Id == request.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, request.IsRead), ct);

        return updated == 0
            ? Error.NotFound("message.not_found", "Mesaj bulunamadı.")
            : Result.Success();
    }
}

public sealed record DeleteMessageCommand(Guid Id) : IRequest<Result>;

internal sealed class DeleteMessageCommandHandler(IAppDbContext db) : IRequestHandler<DeleteMessageCommand, Result>
{
    public async Task<Result> Handle(DeleteMessageCommand request, CancellationToken ct)
    {
        var deleted = await db.ContactMessages.Where(m => m.Id == request.Id).ExecuteDeleteAsync(ct);
        return deleted == 0
            ? Error.NotFound("message.not_found", "Mesaj bulunamadı.")
            : Result.Success();
    }
}
