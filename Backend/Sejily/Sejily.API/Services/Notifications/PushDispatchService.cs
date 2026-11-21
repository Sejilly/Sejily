using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sejily.API.Data;
using Sejily.API.Models.Enums;

namespace Sejily.API.Services.Notifications;

/// <summary>
/// T18: background worker that turns committed notifications into pushes
/// (plan section 3.3). For each job it opens a scope, reloads the notification
/// with the names needed for wording (ONE query, no N+1), composes the push text
/// with the SAME NotificationComposer the REST list uses (decision D4) and sends
/// to every token of the recipient. Invalid tokens are deleted (criterion 10);
/// every error is logged and swallowed — push failures never affect any request
/// and there are no retries in v1 (REST is the source of truth).
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
                    // Retries: none in v1 (plan section 3.3) — log and continue.
                    _logger.LogError(ex,
                        "Push dispatch failed for notification {NotificationId}.",
                        job.NotificationId);
                }
            }
        }
    }

    /// <summary>
    /// Processes one job inside the given scope. Public so T19 can test the
    /// whole path (compose, send, invalid-token delete, error swallow) without
    /// running a host.
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

        // Spec section 8: data payload the app uses to open the request page.
        // All values are strings; requestId is "" when the notification has none.
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
                    // Criterion 10: FCM reported the token unregistered/invalid.
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
                // One broken token must not stop the others (plan section 3.3).
                _logger.LogError(ex, "Push sender threw for token {DeviceTokenId}.", device.DeviceTokenId);
            }
        }
    }

    /// <summary>Projection row for one push job (kept private to this service).</summary>
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
