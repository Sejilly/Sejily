namespace Sejily.API.Models.Entities;

public class PasswordResetOtp
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string OtpHash { get; set; } = null!;

    public DateTime ExpiresAtUtc { get; set; }

    public int FailedAttempts { get; set; }

    public bool OtpConsumed { get; set; }

    public DateTime? VerifiedAtUtc { get; set; }

    public string? ResetTokenHash { get; set; }

    public DateTime? ResetTokenExpiresAtUtc { get; set; }

    public bool ResetUsed { get; set; }
}