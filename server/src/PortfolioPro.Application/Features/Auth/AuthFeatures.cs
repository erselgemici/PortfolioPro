using FluentValidation;
using MediatR;
using PortfolioPro.Application.Common;
using PortfolioPro.Application.Common.Abstractions;
using PortfolioPro.Application.Common.Models;

namespace PortfolioPro.Application.Features.Auth;

// ---------- Giriş ----------
public sealed record LoginCommand(string Email, string Password) : IRequest<Result<AuthTokens>>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(150);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(128);
    }
}

internal sealed class LoginCommandHandler(IAuthService auth) : IRequestHandler<LoginCommand, Result<AuthTokens>>
{
    public Task<Result<AuthTokens>> Handle(LoginCommand request, CancellationToken ct) =>
        auth.LoginAsync(request.Email.Trim(), request.Password, ct);
}

// ---------- Token yenileme ----------
public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<Result<AuthTokens>>;

internal sealed class RefreshTokenCommandHandler(IAuthService auth)
    : IRequestHandler<RefreshTokenCommand, Result<AuthTokens>>
{
    public Task<Result<AuthTokens>> Handle(RefreshTokenCommand request, CancellationToken ct) =>
        auth.RefreshAsync(request.RefreshToken, ct);
}

// ---------- Çıkış ----------
public sealed record LogoutCommand(string? RefreshToken) : IRequest;

internal sealed class LogoutCommandHandler(IAuthService auth) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(request.RefreshToken))
            await auth.RevokeAsync(request.RefreshToken, ct);
    }
}

// ---------- Şifre değiştir ----------
public sealed record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword) : IRequest<Result>;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(10).WithMessage("Şifre en az 10 karakter olmalıdır.")
            .Matches("[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir.")
            .Matches("[a-z]").WithMessage("Şifre en az bir küçük harf içermelidir.")
            .Matches("[0-9]").WithMessage("Şifre en az bir rakam içermelidir.")
            .NotEqual(x => x.CurrentPassword).WithMessage("Yeni şifre eskisiyle aynı olamaz.");
    }
}

internal sealed class ChangePasswordCommandHandler(IAuthService auth) : IRequestHandler<ChangePasswordCommand, Result>
{
    public Task<Result> Handle(ChangePasswordCommand request, CancellationToken ct) =>
        auth.ChangePasswordAsync(request.UserId, request.CurrentPassword, request.NewPassword, ct);
}
