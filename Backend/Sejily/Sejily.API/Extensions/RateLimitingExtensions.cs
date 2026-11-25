using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Sejily.API.Settings;

namespace Sejily.API.Extensions;

// The rate-limit policies used by the endpoints. Requests are counted per logged-in user
// (GetLoggedInUserId()), so a guardian cannot get around the limit by switching accounts;
// anonymous requests fall back to the client IP.
public static class RateLimitingExtensions
{
    // Attribute usage: [EnableRateLimiting(RateLimitingExtensions.General)].
    public const string General = "general";
    public const string Write = "write";
    public const string EmergencyRespond = "emergency-respond";

    public static IServiceCollection AddSejilyRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, cancellationToken) =>
            {
                // The lease knows when the window frees up; the app waits for that many seconds.
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((TimeSpan)retryAfter).TotalSeconds.ToString("0", CultureInfo.InvariantCulture);
                }

                await context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "Too many requests",
                    Detail = "محاولات كتيرة، حاول بعد قليل",
                    Extensions = { ["code"] = "rate_limited" },
                }, cancellationToken);
            };

            options.AddPolicy(General,
                context => CreatePartition(context, limits => limits.GeneralPerMinute));
            options.AddPolicy(Write,
                context => CreatePartition(context, limits => limits.WritePerMinute));
            options.AddPolicy(EmergencyRespond,
                context => CreatePartition(context, limits => limits.EmergencyRespondPerMinute));
        });

        return services;
    }

    // One sliding window per user (or IP). The limiter is cached per key, so the counter
    // survives across requests; a 1-minute window split into 6 segments slides smoothly.
    private static RateLimitPartition<string> CreatePartition(
        HttpContext context, Func<RateLimitSettings, int> limit)
    {
        var settings = context.RequestServices
            .GetRequiredService<IOptions<RateLimitSettings>>().Value;

        return RateLimitPartition.GetSlidingWindowLimiter(
            PartitionKey(context),
            _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = limit(settings),
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueLimit = 0,
            });
    }

    private static string PartitionKey(HttpContext context) =>
        context.User.GetLoggedInUserId()?.ToString(CultureInfo.InvariantCulture)
        ?? context.Connection.RemoteIpAddress?.ToString()
        ?? "anonymous";
}
