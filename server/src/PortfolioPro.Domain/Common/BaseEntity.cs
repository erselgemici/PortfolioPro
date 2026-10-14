namespace PortfolioPro.Domain.Common;

public abstract class BaseEntity
{
    // NOT: Guid.CreateVersion7() .NET 9 ile geldi. Zamana göre sıralı Guid üretiyor,
    // bu sayede SQL Server'da index parçalanması (fragmentation) normal Guid'e göre çok daha az oluyor
    public Guid Id { get; set; } = Guid.CreateVersion7();

    // CreatedAt ve UpdatedAt'i elle doldurmuyoruz, AuditInterceptor SaveChanges anında dolduruyor
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
