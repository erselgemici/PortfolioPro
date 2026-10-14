using PortfolioPro.Domain.Common;

namespace PortfolioPro.Domain.Entities;

public class Experience : BaseEntity
{
    public string Company { get; set; } = string.Empty;
    public LocalizedText Position { get; set; } = LocalizedText.Empty;
    public LocalizedText Description { get; set; } = LocalizedText.Empty;
    public string? Location { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; } // null ise hala burada çalışıyorum demek
    public List<string> Technologies { get; set; } = [];
    public int DisplayOrder { get; set; }
}
