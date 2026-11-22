using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sejily.API.Data;
using Sejily.API.Models.Enums;

namespace Sejily.API.Services.Notifications;

/// <summary>
/// Background worker that actually sends the pushes. It runs for as long as the app runs.
///
/// How it works: when a save is committed, the interceptor drops a PushJob (just the notification
/// id) into a queue (Channel). This worker takes the jobs from the queue one by one, loads what it
/// needs from the database, writes the push text, and sends it to every device of the recipient.
///
/// It runs separately from the user's request, so a slow or failing Firebase never slows down or
/// breaks the request. Errors are only logged and there are no retries (the notification is still
/// available through the REST API).
/// </summary>
public class PushDispatchService : BackgroundService
{
    private readonly Channel<PushJob> _channel;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PushDispatchService> _logger;

    public PushDispatchService(
        Channel<PushJob> channel,
        IServiceScopeFactory scopeFactory,
        ILogger<PushDispatchService> logger)
    {
        _channel = channel;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _channel.Reader.WaitToReadAsync(stoppingToken))
        {
            while (_channel.Reader.TryRead(out var job))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    await HandleJobAsync(scope.ServiceProvider, job.NotificationId, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    // Don't crash the worker: log the error and move on to the next job.
                    _logger.LogError(ex,
                        "Push dispatch failed for notification {NotificationId}.",
                        job.NotificationId);
                }
            }
        }
    }

    /// <summary>
    /// Handles ONE job: loads the notification, writes the push text, sends it to all the recipient's
    /// devices, and deletes the tokens that Firebase says are dead.
    /// Public so tests can call it directly without starting the whole app.
    /// </summary>
    public async Task HandleJobAsync(
        IServiceProvider services,
        int notificationId,
        CancellationToken cancellationToken = default)
    {
        var db = services.GetRequiredService<SejilyDbContext>();
        var composer = services.GetRequiredService<NotificationComposer>();
        var nameMasker = services.GetRequiredService<INameMasker>();
        var sender = services.GetRequiredService<IPushSender>();

        var row = await db.Notifications
            .AsNoTracking()
            .Where(n => n.NotificationId == notificationId)
            .Select(n => new PushRow
            {
                Type = n.Type,
                RequestId = n.RequestId,
                RecipientUserId = n.RecipientUserId,
                SubjectPatientId = n.SubjectPatientId,
                RecipientIsDoctor = n.RecipientUser.AccountType == AccountType.Doctor,
                DoctorName = n.Request != null ? n.Request.Doctor.User.FullName : null,
                DoctorSpecialization = n.Request != null ? n.Request.Doctor.Specialization : null,
                WorkplaceName = n.Request != null ? n.Request.Workplace.Name : null,
                RecordOwnerName = n.Request != null
                    ? n.Request.RecipientPatient.User.FullName
                    : (n.SubjectPatient != null ? n.SubjectPatient.User.FullName : null),
                DependentName = n.SubjectPatient != null ? n.SubjectPatient.User.FullName : null
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (row == null)
        {
            _logger.LogWarning(
                "Push job {NotificationId}: notification no longer exists; skipping.",
                notificationId);
            return;
        }

        var role = row.RecipientIsDoctor
            ? NotificationRecipientRole.Doctor
            : (row.SubjectPatientId != null
                ? NotificationRecipientRole.Guardian
                : NotificationRecipientRole.Patient);

        var (title, body) = composer.ComposePush(new NotificationRenderData
        {
            Type = row.Type,
            RecipientRole = role,
            DoctorName = row.DoctorName ?? string.Empty,
            DoctorSpecialization = row.DoctorSpecialization ?? string.Empty,
            MaskedPatientName = row.RecordOwnerName == null
                ? string.Empty
                : nameMasker.Mask(row.RecordOwnerName),
            DependentName = row.DependentName,
            WorkplaceName = row.WorkplaceName ?? string.Empty,
            LabType = null
        });

        // Extra data sent with the push. When the user taps it, the mobile app reads "requestId" to open
        // the request page. Firebase only accepts strings ("" when there is no request).
        var data = new Dictionary<string, string>
        {
            ["notificationId"] = notificationId.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["type"] = row.Type.ToString(),
            ["requestId"] = row.RequestId?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty
        };

        var tokens = await db.DeviceTokens
            .AsNoTracking()
            .Where(d => d.UserId == row.RecipientUserId)
            .Select(d => new { d.DeviceTokenId, d.Token })
            .ToListAsync(cancellationToken);

        foreach (var device in tokens)
        {
            try
            {
                var result = await sender.SendAsync(device.Token, title, body, data, cancellationToken);

                if (result == PushSendResult.InvalidToken)
                {
                    // Firebase says the token is dead (for example the app was uninstalled): delete it.
                    await db.DeviceTokens
                        .Where(d => d.DeviceTokenId == device.DeviceTokenId)
                        .ExecuteDeleteAsync(cancellationToken);
                    _logger.LogInformation(
                        "Deleted invalid device token {DeviceTokenId} after FCM rejection.",
                        device.DeviceTokenId);
                }
            }
            catch (Exception ex)
            {
                // One failing token must not stop the others.
                _logger.LogError(ex, "Push sender threw for token {DeviceTokenId}.", device.DeviceTokenId);
            }
        }
    }

    /// <summary>The few values needed to write the push text for one job.</summary>
    private sealed class PushRow
    {
        public Models.Enums.Notifications.NotificationType Type { get; set; }
        public int? RequestId { get; set; }
        public int RecipientUserId { get; set; }
        public int? SubjectPatientId { get; set; }
        public bool RecipientIsDoctor { get; set; }
        public string? DoctorName { get; set; }
        public string? DoctorSpecialization { get; set; }
        public string? WorkplaceName { get; set; }
        public string? RecordOwnerName { get; set; }
        public string? DependentName { get; set; }
    }
}
