using Sejily.API.Models.Entities;

namespace Sejily.API.Services.Notifications;

/// <summary>
/// A waiting list of pushes that must NOT be sent yet.
///
/// Why: when we create a notification we don't know yet whether the database save will succeed.
/// If we sent the push now and the save failed later, the user would get a notification about
/// something that never happened. So we write the notification here first, and send the push only
/// after the save is committed.
///
/// Each entry also remembers the id of the explicit transaction that was open when it was added
/// (or null if there was none). That way a commit only releases ITS OWN entries, and entries from
/// an abandoned transaction are never sent by mistake.
///
/// It is "scoped": one instance per web request, shared by NotificationService and the interceptors.
/// </summary>
public class PendingPushCollector
{
    private readonly List<(Notification Notification, Guid? TransactionId)> _pending = new();

    public void Add(Notification notification, Guid? transactionId = null) =>
        _pending.Add((notification, transactionId));

    /// <summary>
    /// Called after a COMMIT. Takes out (and removes) the entries that belong to the committed
    /// transaction, plus the entries that have no transaction id. Entries of other transactions stay.
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
    /// Takes out (and removes) the entries that have no explicit transaction, i.e. they were saved by a
    /// plain SaveChanges. Needed because EF does not open a transaction for a single-statement save, so
    /// there is no commit event for it.
    /// </summary>
    public IReadOnlyList<Notification> DrainWithoutTransaction()
    {
        var released = _pending.Where(p => p.TransactionId == null).Select(p => p.Notification).ToArray();
        _pending.RemoveAll(p => p.TransactionId == null);
        return released;
    }

    /// <summary>A plain SaveChanges failed, so nothing was saved: forget those pushes.</summary>
    public void ClearWithoutTransaction() => _pending.RemoveAll(p => p.TransactionId == null);

    /// <summary>
    /// Called after a ROLLBACK: the rows were not kept, so drop their pushes too (nothing is sent).
    /// </summary>
    public void Clear(Guid? rolledBackTransactionId = null) =>
        _pending.RemoveAll(p => p.TransactionId == null
                             || rolledBackTransactionId == null
                             || p.TransactionId == rolledBackTransactionId);
}
