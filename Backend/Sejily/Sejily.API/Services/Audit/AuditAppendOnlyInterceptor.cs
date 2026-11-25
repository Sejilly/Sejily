using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Sejily.API.Models.Entities;

namespace Sejily.API.Services.Audit;

// AuditLog rows are append-only: inserted once, never changed again. The database has no
// rule that enforces this, so this interceptor stops any save that would update or delete
// one, before the SQL is even sent.
public class AuditAppendOnlyInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        ThrowIfAuditLogChanged(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ThrowIfAuditLogChanged(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void ThrowIfAuditLogChanged(DbContext? context)
    {
        var changed = context?.ChangeTracker.Entries<AuditLog>()
            .Where(e => e.State is EntityState.Modified or EntityState.Deleted)
            .ToList();

        if (changed is { Count: > 0 })
        {
            throw new InvalidOperationException(
                "AuditLog is append-only: rows cannot be updated or deleted.");
        }
    }
}
