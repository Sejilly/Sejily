using System.Data.Common;
using System.Threading.Channels;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Sejily.API.Services.Notifications;

/// <summary>
/// An EF Core "interceptor": code that EF calls automatically when something happens to a transaction.
/// Here we listen for COMMIT and ROLLBACK:
///  - Commit: the data is safely saved, so move the waiting pushes into the queue to be sent.
///  - Rollback: the data was cancelled, so throw the waiting pushes away (no push is sent).
/// This is why the code that creates notifications never has to send pushes itself.
/// (A save that needs no transaction is handled by PushOnSaveInterceptor.)
/// </summary>
public class PushOnCommitInterceptor : DbTransactionInterceptor
{
    private readonly PendingPushCollector _collector;
    private readonly Channel<PushJob> _channel;
    private readonly ILogger<PushOnCommitInterceptor> _logger;

    public PushOnCommitInterceptor(
        PendingPushCollector collector,
        Channel<PushJob> channel,
        ILogger<PushOnCommitInterceptor> logger)
    {
        _collector = collector;
        _channel = channel;
        _logger = logger;
    }

    public override void TransactionCommitted(DbTransaction transaction, TransactionEndEventData eventData) =>
        Flush(eventData.TransactionId);

    public override Task TransactionCommittedAsync(
        DbTransaction transaction, TransactionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        Flush(eventData.TransactionId);
        return Task.CompletedTask;
    }

    public override void TransactionRolledBack(DbTransaction transaction, TransactionEndEventData eventData) =>
        _collector.Clear(eventData.TransactionId);

    public override Task TransactionRolledBackAsync(
        DbTransaction transaction, TransactionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        _collector.Clear(eventData.TransactionId);
        return Task.CompletedTask;
    }

    private void Flush(Guid committedTransactionId)
    {
        foreach (var notification in _collector.Drain(committedTransactionId))
        {
            if (notification.NotificationId == 0)
            {
                // This row was never saved (it has no Id yet), so there is nothing to push.
                _logger.LogWarning(
                    "Skipping push for a {Type} notification that was never saved (Id == 0).",
                    notification.Type);
                continue;
            }

            _channel.Writer.TryWrite(new PushJob(notification.NotificationId));
        }
    }
}
