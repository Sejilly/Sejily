using Microsoft.EntityFrameworkCore;
using Sejily.API.Data;
using Sejily.API.DTOs.Notifications;
using Sejily.API.Models.Enums;
using Sejily.API.Models.Enums.Notifications;

namespace Sejily.API.Services.Notifications;

/// <summary>
/// T07: list query. ONE projection query fetches the notification plus the names
/// needed for composition (plan section 3.5, no N+1); wording is composed in
/// memory by NotificationComposer (plan decision D4). Paging is keyset-based
/// (plan decision D6): (CreatedAt &lt; c) OR (CreatedAt = c AND Id &lt; id),
/// ordered CreatedAt DESC, Id DESC, taking limit + 1 rows to detect the next page.
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
            // Invalid cursors throw InvalidNotificationCursorException; the
            // controller maps it to 400 ProblemDetails (spec section 5).
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
                // The record owner whose name is (masked and) shown in doctor-facing
                // bodies: the request's recipient patient, or the dependent when the
                // notification is about a guardian's dependent.
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
                // Reserved types (LabResult / GuardianLinked / MedicalVisit) have no
                // request, so doctor/workplace data is absent; composer wording for
                // those types never uses it.
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
                // Review fix 2: rows read from SQL Server arrive with Kind =
                // Unspecified; force UTC so JSON serializes ISO-8601 with the
                // "Z" suffix (spec section 4.1). The cursor does the same in
                // NotificationCursor.Encode.
                CreatedAt = DateTime.SpecifyKind(row.CreatedAt, DateTimeKind.Utc),
                RequestId = row.RequestId,
                SubjectPatientId = row.SubjectPatientId
            });
        }

        // The cursor points at the LAST row of this page; the next page continues
        // strictly before it (keyset), so paging has no duplicates or gaps even if
        // new notifications arrive between requests (spec acceptance criterion 2).
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

    // ================= T08: unread-count / read / read-all / delete (plan 3.5) =================

    public Task<int> CountUnreadAsync(int userId, CancellationToken cancellationToken = default) =>
        _db.Notifications.CountAsync(
            n => n.RecipientUserId == userId && n.DeletedAt == null && !n.IsRead,
            cancellationToken);

    public async Task<NotificationUpdateResult> MarkReadAsync(
        int userId, int notificationId, CancellationToken cancellationToken = default)
    {
        // ExecuteUpdate touches 0 rows when the id is unknown, not mine, or already
        // soft-deleted => NotFound (404 in the controller). Repeating read on an
        // already-read row is safe: it just writes the same value (criterion 4).
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
        // Repeat-safe: unread-only predicate, so a second call updates nothing.
        await _db.Notifications
            .Where(n => n.RecipientUserId == userId && n.DeletedAt == null && !n.IsRead)
            .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true),
                cancellationToken);
    }

    public async Task<NotificationUpdateResult> DeleteAsync(
        int userId, int notificationId, CancellationToken cancellationToken = default)
    {
        // Soft delete only (DeletedAt); the request is never touched (criterion 5).
        // Already-deleted rows return NotFound (open question 2 default: 404).
        var deletedAt = _timeProvider.GetUtcNow().UtcDateTime;
        var rows = await _db.Notifications
            .Where(n => n.NotificationId == notificationId
                     && n.RecipientUserId == userId
                     && n.DeletedAt == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.DeletedAt, deletedAt),
                cancellationToken);

        return rows == 0 ? NotificationUpdateResult.NotFound : NotificationUpdateResult.Success;
    }

    /// <summary>Projection row used inside ListAsync (kept private to this service).</summary>
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
