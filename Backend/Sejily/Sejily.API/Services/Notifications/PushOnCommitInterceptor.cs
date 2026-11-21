using System.Data.Common;
using System.Threading.Channels;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Sejily.API.Services.Notifications;

/// <summary>
/// T16: releases pending pushes on transaction commit (plan decision D3).
/// Registered as a SCOPED service and resolved inside the AddDbContext lambda,
/// so its PendingPushCollector is the SAME instance the NotificationService of
/// the current request used. Commit => one PushJob per saved row; rollback =>
/// the collector is cleared and nothing is enqueued.
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
        Flush();

    public override Task TransactionCommittedAsync(
        DbTransaction transaction, TransactionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        Flush();
        return Task.CompletedTask;
    }

    public override void TransactionRolledBack(DbTransaction transaction, TransactionEndEventData eventData) =>
        _collector.Clear();

    public override Task TransactionRolledBackAsync(
        DbTransaction transaction, TransactionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        _collector.Clear();
        return Task.CompletedTask;
    }

    private void Flush()
    {
        foreach (var notification in _collector.Drain())
        {
            if (notification.NotificationId == 0)
            {
                // The row was never saved (plan section 3.2) — nothing to push.
                _logger.LogWarning(
                    "Skipping push for a {Type} notification that was never saved (Id == 0).",
                    notification.Type);
                continue;
            }

            _channel.Writer.TryWrite(new PushJob(notification.NotificationId));
        }
    }
}
