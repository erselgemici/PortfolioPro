using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PortfolioPro.Domain.Common;

namespace PortfolioPro.Infrastructure.Persistence.Interceptors;

// SaveChanges çağrıldığı an araya girip CreatedAt / UpdatedAt alanlarını dolduruyor.
// NOT: Her handler'da "entity.CreatedAt = DateTime.UtcNow" yazmak yerine tek yerden yönetiliyor.
// DateTime.UtcNow yerine TimeProvider kullandım, testlerde saati istediğim değere sabitleyebilmek için.
public sealed class AuditInterceptor(TimeProvider time) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            ApplyAudit(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
            ApplyAudit(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    private void ApplyAudit(DbContext context)
    {
        var now = time.GetUtcNow().UtcDateTime;

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = now;
            else if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = now;
        }
    }
}
