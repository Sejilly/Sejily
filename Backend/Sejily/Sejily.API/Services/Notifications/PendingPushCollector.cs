using Sejily.API.Models.Entities;

namespace Sejily.API.Services.Notifications;

/// <summary>
/// T16: SCOPED collector for pushes waiting for the caller's transaction to commit
/// (plan decision D3). NotificationService adds entity references here; the
/// PushOnCommitInterceptor (also scoped, so it sees the SAME instance) drains it
/// on TransactionCommitted and clears it on TransactionRolledBack.
/// </summary>
public class PendingPushCollector
{
    private readonly List<Notification> _pending = new();

    public void Add(Notification notification) => _pending.Add(notification);

    /// <summary>Returns everything collected and empties the collector.</summary>
    public IReadOnlyList<Notification> Drain()
    {
        var copy = _pending.ToArray();
        _pending.Clear();
        return copy;
    }

    /// <summary>Called on rollback: the rows are gone, so the pushes must be too.</summary>
    public void Clear() => _pending.Clear();
}
