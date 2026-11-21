using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Sejily.API.Data;

// SQL Server datetime2 comes back as DateTimeKind.Unspecified, so System.Text.Json
// writes it WITHOUT "Z" and clients may read it as local time (wrong by 2-3 hours in Egypt).
// These converters make every DateTime we read UTC-flagged, so the API emits "...Z".
// Rule: everything stored in the database is UTC.

public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter() : base(
        // write: Local -> UTC, Unspecified is assumed to already be UTC
        v => v.Kind == DateTimeKind.Local ? v.ToUniversalTime() : v,
        // read: flag as UTC
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    { }
}

public class NullableUtcDateTimeConverter : ValueConverter<DateTime?, DateTime?>
{
    public NullableUtcDateTimeConverter() : base(
        v => v.HasValue && v.Value.Kind == DateTimeKind.Local ? v.Value.ToUniversalTime() : v,
        v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v)
    { }
}
