using System.Threading.Channels;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Sejily.API.Services.Notifications;

/// <summary>
/// Companion of PushOnCommitInterceptor, for saves that happen WITHOUT a transaction.
///
/// EF does not open a transaction when a SaveChanges needs only one SQL statement (for example when
/// only the notification is inserted). Then there is no "commit" event, so the commit interceptor
/// would never send the push.
///
/// So after a SaveChanges succeeds and there is no explicit transaction, the data is already
/// committed and we release the waiting pushes. If there IS an explicit transaction we do nothing
/// here; PushOnCommitInterceptor sends them when it commits. If the save fails we throw them away.
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
