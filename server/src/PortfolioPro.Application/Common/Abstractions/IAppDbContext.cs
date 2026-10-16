using Microsoft.EntityFrameworkCore;
using PortfolioPro.Domain.Entities;

namespace PortfolioPro.Application.Common.Abstractions;

// NOT: DevLog'da Generic Repository + Unit of Work yazmıştım. Burada daha sade bir yol denedim:
// EF Core'un DbContext'i değişiklikleri zaten toplu kaydediyor (Unit of Work), DbSet'ler de repository gibi çalışıyor.
// Application katmanı sadece bu arayüzü biliyor, gerçek AppDbContext Infrastructure'da.
// Böylece handler'larda Select ile sadece ihtiyacım olan kolonları çekebiliyorum.
public interface IAppDbContext
{
    DbSet<SiteProfile> Profiles { get; }
    DbSet<Project> Projects { get; }
    DbSet<Experience> Experiences { get; }
    DbSet<Education> Educations { get; }
    DbSet<Skill> Skills { get; }
    DbSet<Certificate> Certificates { get; }
    DbSet<ContactMessage> ContactMessages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
