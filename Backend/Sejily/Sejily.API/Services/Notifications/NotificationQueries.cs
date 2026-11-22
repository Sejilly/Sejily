using Microsoft.EntityFrameworkCore;
using Sejily.API.Data;
using Sejily.API.DTOs.Notifications;
using Sejily.API.Models.Enums;
using Sejily.API.Models.Enums.Notifications;

namespace Sejily.API.Services.Notifications;

/// <summary>
/// Reads the current user's notifications for the REST endpoints.
/// The list is fetched with ONE database query that also brings the names needed to write each
/// title and body (doctor, patient, workplace); the text itself is then built in memory by
/// NotificationComposer.
/// Paging uses a cursor: "give me the notifications older than the last one I saw", newest first.
/// </summary>
public class NotificationQueries : INotificationQueries
{
    private readonly SejilyDbContext _db;
    private readonly INameMasker _nameMasker;
    private readonly NotificationComposer _composer;
    private readonly TimeProvider _timeProvider;

    public NotificationQueries(
        SejilyDbContext db,
        INameMasker nameMasker,
        NotificationComposer composer,
        TimeProvider timeProvider)
    {
        _db = db;
        _nameMasker = nameMasker;
        _composer = composer;
        _timeProvider = timeProvider;
    }

    public async Task<NotificationListResponse> ListAsync(
        int userId,
        int limit,
        string? cursor,
        bool unreadOnly,
        CancellationToken cancellationToken = default)
    {
        var cursorCreatedAt = default(DateTime?);
        var cursorId = 0;

        if (!string.IsNullOrEmpty(cursor))
        {
            // A bad cursor throws InvalidNotificationCursorException; the controller turns it into a 400.
            (var decodedCreatedAt, cursorId) = NotificationCursor.Decode(cursor);
            cursorCreatedAt = decodedCreatedAt;
        }

        var query = _db.Notifications
            .AsNoTracking()
            .Where(n => n.RecipientUserId == userId && n.DeletedAt == null);

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        if (cursorCreatedAt != null)
        {
            query = query.Where(n =>
                n.CreatedAt < cursorCreatedAt ||
                (n.CreatedAt == cursorCreatedAt && n.NotificationId < cursorId));
        }

        var rows = await query
            .OrderByDescending(n => n.CreatedAt)
            .ThenByDescending(n => n.NotificationId)
            .Take(limit + 1)
            .Select(n => new NotificationRow
            {
                NotificationId = n.NotificationId,
                Type = n.Type,
                CreatedAt = n.CreatedAt,
                IsRead = n.IsRead,
                RequestId = n.RequestId,
                SubjectPatientId = n.SubjectPatientId,
                RecipientIsDoctor = n.RecipientUser.AccountType == AccountType.Doctor,
                DoctorName = n.Request != null ? n.Request.Doctor.User.FullName : null,
                DoctorSpecialization = n.Request != null ? n.Request.Doctor.Specialization : null,
                WorkplaceName = n.Request != null ? n.Request.Workplace.Name : null,
                // The name of the patient the record belongs to (shown masked to doctors): the request's patient,
                // or the dependent when this notification is about a guardian's dependent.
                RecordOwnerName = n.Request != null
                    ? n.Request.RecipientPatient.User.FullName
                    : (n.SubjectPatient != null ? n.SubjectPatient.User.FullName : null),
                DependentName = n.SubjectPatient != null ? n.SubjectPatient.User.FullName : null
            })
            .ToListAsync(cancellationToken);

        var hasNextPage = rows.Count > limit;
        if (hasNextPage)
        {
            rows.RemoveAt(rows.Count - 1);
        }

        var items = new List<NotificationResponse>(rows.Count);
        foreach (var row in rows)
        {
            var role = row.RecipientIsDoctor
                ? NotificationRecipientRole.Doctor
                : (row.SubjectPatientId != null
                    ? NotificationRecipientRole.Guardian
                    : NotificationRecipientRole.Patient);

            var (title, body) = _composer.Compose(new NotificationRenderData
            {
                Type = row.Type,
                RecipientRole = role,
                // Notifications that are not tied to a request (LabResult, GuardianLinked, MedicalVisit) have no
                // doctor or workplace data; their text never uses it.
                DoctorName = row.DoctorName ?? string.Empty,
                DoctorSpecialization = row.DoctorSpecialization ?? string.Empty,
                MaskedPatientName = row.RecordOwnerName == null
                    ? string.Empty
                    : _nameMasker.Mask(row.RecordOwnerName),
                DependentName = row.DependentName,
                WorkplaceName = row.WorkplaceName ?? string.Empty,
                LabType = null
            });

            items.Add(new NotificationResponse
            {
                Id = row.NotificationId,
                Type = row.Type,
                Title = title,
                Body = body,
                IsRead = row.IsRead,
                // SQL Server returns dates without a timezone flag. Mark it as UTC so the JSON ends with "Z"
                // (example: 2026-11-17T09:30:00Z) and the app converts it to local time correctly.
                CreatedAt = DateTime.SpecifyKind(row.CreatedAt, DateTimeKind.Utc),
                RequestId = row.RequestId,
                SubjectPatientId = row.SubjectPatientId
            });
        }

        // The cursor points at the LAST item of this page; the next page continues right after it.
        // Because it is based on time + id (not a page number), a notification that arrives between two
        // requests can't cause duplicates or missing items.
        var last = rows.Count > 0 ? rows[^1] : null;
        var nextCursor = hasNextPage && last != null
            ? NotificationCursor.Encode(last.CreatedAt, last.NotificationId)
            : null;

        return new NotificationListResponse
        {
            Items = items,
            NextCursor = nextCursor
        };
    }

    // ---- unread count, mark as read, mark all as read, delete ----

    public Task<int> CountUnreadAsync(int userId, CancellationToken cancellationToken = default) =>
        _db.Notifications.CountAsync(
            n => n.RecipientUserId == userId && n.DeletedAt == null && !n.IsRead,
            cancellationToken);

    public async Task<NotificationUpdateResult> MarkReadAsync(
        int userId, int notificationId, CancellationToken cancellationToken = default)
    {
        // Updates the row directly in the database. If 0 rows changed, the id doesn't exist, isn't mine,
        // or was deleted, so we answer "not found". Marking an already-read row again is harmless.
        var rows = await _db.Notifications
            .Where(n => n.NotificationId == notificationId
                     && n.RecipientUserId == userId
                     && n.DeletedAt == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true),
                cancellationToken);

        return rows == 0 ? NotificationUpdateResult.NotFound : NotificationUpdateResult.Success;
    }

    public async Task MarkAllReadAsync(int userId, CancellationToken cancellationToken = default)
    {
        // Only unread rows are selected, so running it twice changes nothing the second time.
        await _db.Notifications
            .Where(n => n.RecipientUserId == userId && n.DeletedAt == null && !n.IsRead)
            .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true),
                cancellationToken);
    }

    public async Task<NotificationUpdateResult> DeleteAsync(
        int userId, int notificationId, CancellationToken cancellationToken = default)
    {
        // Soft delete: only fill DeletedAt. The row stays and the request is not touched.
        // A row that is already deleted returns NotFound.
        var deletedAt = _timeProvider.GetUtcNow().UtcDateTime;
        var rows = await _db.Notifications
            .Where(n => n.NotificationId == notificationId
                     && n.RecipientUserId == userId
                     && n.DeletedAt == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.DeletedAt, deletedAt),
                cancellationToken);

        return rows == 0 ? NotificationUpdateResult.NotFound : NotificationUpdateResult.Success;
    }

    /// <summary>The few values read from the database for one notification row.</summary>
    private sealed class NotificationRow
    {
        public int NotificationId { get; set; }
        public NotificationType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public int? RequestId { get; set; }
        public int? SubjectPatientId { get; set; }
        public bool RecipientIsDoctor { get; set; }
        public string? DoctorName { get; set; }
        public string? DoctorSpecialization { get; set; }
        public string? WorkplaceName { get; set; }
        public string? RecordOwnerName { get; set; }
        public string? DependentName { get; set; }
    }
}
