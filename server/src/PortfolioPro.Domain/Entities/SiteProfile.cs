using PortfolioPro.Domain.Common;

namespace PortfolioPro.Domain.Entities;

// Sitenin sahibine ait bilgiler. Tabloda tek satır olacak.
public class SiteProfile : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public LocalizedText Title { get; set; } = LocalizedText.Empty;     // "Full Stack Developer"
    public LocalizedText Headline { get; set; } = LocalizedText.Empty;  // hero altındaki kısa cümle
    public LocalizedText About { get; set; } = LocalizedText.Empty;     // hakkımda paragrafı
    public string Location { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? CvUrlTr { get; set; }
    public string? CvUrlEn { get; set; }
    public string? GithubUrl { get; set; }
    public string? LinkedinUrl { get; set; }
    public int YearsOfExperience { get; set; }
    public bool IsOpenToWork { get; set; }
}
