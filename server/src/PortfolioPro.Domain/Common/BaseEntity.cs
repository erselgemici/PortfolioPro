namespace PortfolioPro.Domain.Common;

public abstract class BaseEntity
{
    // NOT: Guid.CreateVersion7() .NET 9 ile geldi. Ürettiği Guid'ler zamana göre sıralı.
    // Normal Guid tamamen rastgele olduğu için index'e eklerken veritabanını daha çok yoruyor, bu daha düzenli.
    public Guid Id { get; set; } = Guid.CreateVersion7();

    // CreatedAt ve UpdatedAt'i elle doldurmuyoruz, AuditInterceptor SaveChanges anında dolduruyor
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
