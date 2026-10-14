using PortfolioPro.Application.Common.Models;

namespace PortfolioPro.Application.Common.Abstractions;

// Identity ve JWT detayları Infrastructure'da. Application sadece ne yapılacağını biliyor.
public interface IAuthService
{
    Task<Result<AuthTokens>> LoginAsync(string email, string password, CancellationToken ct);
    Task<Result<AuthTokens>> RefreshAsync(string refreshToken, CancellationToken ct);
    Task RevokeAsync(string refreshToken, CancellationToken ct);
    Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken ct);
}
