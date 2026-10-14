using PortfolioPro.Domain.Common;
using PortfolioPro.Domain.Enums;

namespace PortfolioPro.Domain.Entities;

public class Project : BaseEntity
{
    // URL'de id yerine okunabilir slug kullanıyoruz: /projects/devlog
    public string Slug { get; set; } = string.Empty;
    public LocalizedText Title { get; set; } = LocalizedText.Empty;
    public LocalizedText Summary { get; set; } = LocalizedText.Empty;
    public LocalizedText Content { get; set; } = LocalizedText.Empty; // markdown
    public ProjectCategory Category { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? GithubUrl { get; set; }
    public string? LiveUrl { get; set; }
    public string? StoreUrl { get; set; }

    // NOT: EF Core 8+ primitive collection desteği. List<string> ayrı tablo açmadan
    // SQL Server'da JSON olarak tek kolonda saklanıyor: ["C#", "Angular"]
    public List<string> Technologies { get; set; } = [];

    // Projenin yapıldığı tarih (ay/yıl). Siteda projeler buna göre sıralanıyor: en yeni en üstte.
    // NOT: CreatedAt'i kullanmadım çünkü o kaydın veritabanına eklendiği an; eski bir projeyi bugün eklesem en üstte çıkardı.
    public DateOnly ProjectDate { get; set; }

    public bool IsFeatured { get; set; }
    public bool IsPublished { get; set; }
    public int DisplayOrder { get; set; }
}
