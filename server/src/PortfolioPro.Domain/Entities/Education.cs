using PortfolioPro.Domain.Common;

namespace PortfolioPro.Domain.Entities;

public class Education : BaseEntity
{
    public string School { get; set; } = string.Empty;
    public LocalizedText Department { get; set; } = LocalizedText.Empty;
    public LocalizedText Degree { get; set; } = LocalizedText.Empty; // Lisans, Ön Lisans...
    public LocalizedText Description { get; set; } = LocalizedText.Empty;
    public int StartYear { get; set; }
    public int? EndYear { get; set; }
    public int DisplayOrder { get; set; }
}
