using PortfolioPro.Domain.Common;
using PortfolioPro.Domain.Enums;

namespace PortfolioPro.Domain.Entities;

public class Skill : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public SkillCategory Category { get; set; }
    public bool IsHighlighted { get; set; } // ana sayfada öne çıkacak mı
    public int DisplayOrder { get; set; }
}
