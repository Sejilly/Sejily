using System.Buffers.Text;
using System.Globalization;
using System.Text;

namespace Sejily.API.Services.Notifications;

/// <summary>
/// Thrown when the cursor sent by the app can't be understood (empty, random text, or edited).
/// The controller turns it into a 400 Bad Request.
/// </summary>
public class InvalidNotificationCursorException : Exception
{
    public InvalidNotificationCursorException(string message) : base(message)
    {
    }
}

/// <summary>
/// A cursor is a bookmark for paging. Instead of "page 2", the app says "continue after the last
/// notification I saw". It is made of that notification's time and id.
///
/// Example: the last notification seen was id 1042, created at 2026-11-17 09:30 UTC.
/// Encode turns that into the text "2026-11-17T09:30:00.0000000Z|1042" and then into Base64Url
/// (letters and digits only), so the app treats it as an opaque string.
/// Decode does the reverse. The id is included because two notifications can share the same time.
/// </summary>
public static class NotificationCursor
{
    public static string Encode(DateTime createdAtIndexUtc, int notificationId)
    {
        // Dates read from SQL Server carry no timezone flag. We always store UTC, so mark it as UTC;
        // otherwise the text would miss the trailing "Z" that Decode requires.
        var utc = createdAtIndexUtc.Kind == DateTimeKind.Utc
            ? createdAtIndexUtc
            : DateTime.SpecifyKind(createdAtIndexUtc, DateTimeKind.Utc);
        var payload = $"{utc:O}|{notificationId}";
        return Base64Url.EncodeToString(Encoding.UTF8.GetBytes(payload));
    }

    public static (DateTime CreatedAtIndexUtc, int NotificationId) Decode(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
            throw new InvalidNotificationCursorException("Cursor is missing.");

        byte[] bytes;
        try
        {
            bytes = Base64Url.DecodeFromChars(cursor);
        }
        catch (FormatException)
        {
            throw new InvalidNotificationCursorException("Cursor is not valid Base64Url.");
        }

        string payload;
        try
        {
            payload = Encoding.UTF8.GetString(bytes);
        }
        catch (ArgumentException)
        {
            throw new InvalidNotificationCursorException("Cursor payload is not valid UTF-8.");
        }

        var separator = payload.IndexOf('|');
        if (separator < 0)
            throw new InvalidNotificationCursorException("Cursor payload is malformed.");

        var createdAtText = payload[..separator];
        var idText = payload[(separator + 1)..];

        if (!DateTime.TryParse(createdAtText, CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind, out var createdAt) ||
            createdAt.Kind != DateTimeKind.Utc)
            throw new InvalidNotificationCursorException("Cursor timestamp is not a valid UTC instant.");

        if (!int.TryParse(idText, NumberStyles.None, CultureInfo.InvariantCulture, out var id) || id < 1)
            throw new InvalidNotificationCursorException("Cursor id is not a valid notification id.");

        return (createdAt, id);
    }
}
