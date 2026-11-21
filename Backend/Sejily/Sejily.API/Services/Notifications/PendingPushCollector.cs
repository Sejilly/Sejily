using Sejily.API.Models.Entities;

namespace Sejily.API.Services.Notifications;

/// <summary>
/// T16: SCOPED collector for pushes waiting for the caller's transaction to commit
/// (plan decision D3). NotificationService adds entity references here; the
/// PushOnCommitInterceptor (also scoped, so it sees the SAME instance) drains it
/// on TransactionCommitted and clears it on TransactionRolledBack.
///
/// Each entry remembers the id of the EXPLICIT transaction that was active when
/// NotifyAsync ran (null when none: the row is saved by a plain SaveChanges and its
/// implicit transaction). A commit only releases entries of ITS OWN transaction (or
/// null ones), so an explicit transaction that was disposed without Commit/Rollback
/// (no interceptor event) can never leak its pushes into a later, unrelated commit.
/// </summary>
public class PendingPushCollector
{
    private readonly List<(Notification Notification, Guid? TransactionId)> _pending = new();

    public void Add(Notification notification, Guid? transactionId = null) =>
        _pending.Add((notification, transactionId));

    /// <summary>
    /// Returns the entries that belong to the committed transaction (or have no
    /// transaction id) and removes them. Entries of other transactions stay untouched.
    /// </summary>
    public IReadOnlyList<Notification> Drain(Guid? committedTransactionId = null)
    {
        var released = _pending
            .Where(p => p.TransactionId == null
                     || committedTransactionId == null
                     || p.TransactionId == committedTransactionId)
            .ToList();

        _pending.RemoveAll(p => released.Contains(p));
        return released.Select(p => p.Notification).ToArray();
    }

    /// <summary>
    /// Releases the entries that have NO explicit transaction (saved by a plain SaveChanges).
    /// Used by PushOnSaveInterceptor: EF does not open a transaction for a single-statement
    /// SaveChanges, so no TransactionCommitted event exists for it.
    /// </summary>
    public IReadOnlyList<Notification> DrainWithoutTransaction()
    {
        var released = _pending.Where(p => p.TransactionId == null).Select(p => p.Notification).ToArray();
        _pending.RemoveAll(p => p.TransactionId == null);
        return released;
    }

    /// <summary>A plain SaveChanges failed: its rows were not saved, so drop their pushes.</summary>
    public void ClearWithoutTransaction() => _pending.RemoveAll(p => p.TransactionId == null);

    /// <summary>
    /// Called on rollback: the rows of that transaction (and the ones without a
    /// transaction id) are gone, so their pushes must be too.
    /// </summary>
    public void Clear(Guid? rolledBackTransactionId = null) =>
        _pending.RemoveAll(p => p.TransactionId == null
                             || rolledBackTransactionId == null
                             || p.TransactionId == rolledBackTransactionId);
}
