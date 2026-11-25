namespace Sejily.API.Services.Audit;

// Appends audit rows. It never calls SaveChanges: the row is saved together with the
// caller's own change, inside the caller's transaction. If the transaction rolls back,
// the audit row disappears with it.
public interface IAuditService
{
    void Add(AuditEntry entry);
}
