using PortfolioPro.Domain.Common;

namespace PortfolioPro.Domain.Entities;

public class Certificate : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty; // Udemy, BTK Akademi...
    public DateOnly IssuedAt { get; set; }
    public string? CredentialUrl { get; set; }
    public int DisplayOrder { get; set; }
}
