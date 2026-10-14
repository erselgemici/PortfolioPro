namespace PortfolioPro.Application.Common.Models;

public sealed record AdminUserDto(Guid Id, string Email, string FullName);

public sealed record AuthTokens(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    AdminUserDto User);
