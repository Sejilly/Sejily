using System.Buffers.Text;
using System.Globalization;
using System.Text;

namespace Sejily.API.Services.Notifications;

/// <summary>
/// Thrown when a notification list cursor cannot be decoded (missing, garbage or
/// tampered). Controllers map it to 400 ProblemDetails (spec section 5).
/// </summary>
public class InvalidNotificationCursorException : Exception
{
    public InvalidNotificationCursorException(string message) : base(message)
    {
    }
}

/// <summary>
/// T06: opaque pagination cursor (plan decision D6).
/// Wire format: Base64Url of "{CreatedAt:O}|{NotificationId}", e.g.
/// "2026-11-17T09:30:00.0000000Z|1042". The client never parses it; the list
/// query decodes it into the keyset predicate
/// (CreatedAt &lt; c) OR (CreatedAt = c AND Id &lt; id), ordered CreatedAt DESC, Id DESC.
/// </summary>
public static class NotificationCursor
{
    public static string Encode(DateTime createdAtIndexUtc, int notificationId)
    {
        var payload = $"{createdAtIndexUtc:O}|{notificationId}";
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
