using System.Threading.Channels;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Sejily.API.Services.Notifications;

/// <summary>
/// Companion of PushOnCommitInterceptor. EF Core does NOT open a transaction when a
/// SaveChanges needs only ONE statement (e.g. a caller that only inserts the
/// notification), so no TransactionCommitted event fires and the push would be lost.
/// This interceptor covers exactly that case: when a SaveChanges succeeds and the
/// context has NO explicit transaction, the data is already committed, so the pending
/// pushes (those without an explicit transaction id) are released. With an explicit
/// transaction nothing happens here; PushOnCommitInterceptor releases them on Commit.
/// Draining is idempotent, so a multi-statement SaveChanges (implicit transaction,
/// already released by the commit event) is not pushed twice.
/// </summary>
public class PushOnSaveInterceptor : SaveChangesInterceptor
{
    private readonly PendingPushCollector _collector;
    private readonly Channel<PushJob> _channel;
    private readonly ILogger<PushOnSaveInterceptor> _logger;

    public PushOnSaveInterceptor(
        PendingPushCollector collector,
        Channel<PushJob> channel,
        ILogger<PushOnSaveInterceptor> logger)
    {
        _collector = collector;
        _channel = channel;
        _logger = logger;
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (eventData.Context?.Database.CurrentTransaction == null)
        {
            Release();
        }

        return result;
    }

    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context?.Database.CurrentTransaction == null)
        {
            Release();
        }

        return ValueTask.FromResult(result);
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        if (eventData.Context?.Database.CurrentTransaction == null)
        {
            _collector.ClearWithoutTransaction();
        }
    }

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        if (eventData.Context?.Database.CurrentTransaction == null)
        {
            _collector.ClearWithoutTransaction();
        }

        return Task.CompletedTask;
    }

    private void Release()
    {
        foreach (var notification in _collector.DrainWithoutTransaction())
        {
            if (notification.NotificationId == 0)
            {
                _logger.LogWarning(
                    "Skipping push for a {Type} notification that was never saved (Id == 0).",
                    notification.Type);
                continue;
            }

            _channel.Writer.TryWrite(new PushJob(notification.NotificationId));
        }
    }
}
