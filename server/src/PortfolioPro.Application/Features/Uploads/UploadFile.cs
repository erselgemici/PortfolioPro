using FluentValidation;
using MediatR;
using PortfolioPro.Application.Common;
using PortfolioPro.Application.Common.Abstractions;
using PortfolioPro.Application.Common.Helpers;

namespace PortfolioPro.Application.Features.Uploads;

public enum UploadKind
{
    Image = 1,
    Document = 2
}

public sealed record UploadFileCommand(Stream Content, string FileName, long Length, UploadKind Kind)
    : IRequest<Result<string>>;

public sealed class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
{
    public const long MaxImageBytes = 2 * 1024 * 1024;     // 2 MB
    public const long MaxDocumentBytes = 5 * 1024 * 1024;  // 5 MB

    private static readonly string[] ImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly string[] DocumentExtensions = [".pdf"];

    public UploadFileCommandValidator()
    {
        RuleFor(x => x.Kind).IsInEnum();
        RuleFor(x => x.Length).GreaterThan(0).WithMessage("Dosya boş.");

        RuleFor(x => x.Length)
            .LessThanOrEqualTo(MaxImageBytes)
            .When(x => x.Kind == UploadKind.Image)
            .WithMessage("Görsel en fazla 2 MB olabilir.");

        RuleFor(x => x.Length)
            .LessThanOrEqualTo(MaxDocumentBytes)
            .When(x => x.Kind == UploadKind.Document)
            .WithMessage("Doküman en fazla 5 MB olabilir.");

        // NOT: SVG'ye bilerek izin vermedim. SVG aslında XML ve içine <script> konabiliyor.
        RuleFor(x => x.FileName)
            .Must((cmd, name) => AllowedFor(cmd.Kind).Contains(Path.GetExtension(name).ToLowerInvariant()))
            .WithMessage("Bu dosya türüne izin verilmiyor.");
    }

    public static string[] AllowedFor(UploadKind kind) =>
        kind == UploadKind.Image ? ImageExtensions : DocumentExtensions;
}

internal sealed class UploadFileCommandHandler(IFileStorage storage) : IRequestHandler<UploadFileCommand, Result<string>>
{
    public async Task<Result<string>> Handle(UploadFileCommand request, CancellationToken ct)
    {
        var extension = Path.GetExtension(request.FileName).ToLowerInvariant();

        // Uzantı doğru olsa bile içeriğin gerçekten o tip olduğunu kontrol et
        if (!FileSignature.IsValid(request.Content, extension))
            return Error.Validation("upload.invalid_content", "Dosya içeriği uzantısıyla uyuşmuyor.");

        // Kullanıcının verdiği dosya adını HİÇ kullanmıyoruz, storage rastgele isim üretiyor
        var folder = request.Kind == UploadKind.Image ? "images" : "documents";
        var url = await storage.SaveAsync(request.Content, extension, folder, ct);

        return url;
    }
}
