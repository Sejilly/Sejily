using Microsoft.EntityFrameworkCore;
using Sejily.API.Data;
using Sejily.API.Models.Enums;

namespace Sejily.API.Services;

public class KinshipVerificationService : IKinshipVerificationService
{
    private readonly SejilyDbContext _context;

    public KinshipVerificationService(SejilyDbContext context)
    {
        _context = context;
    }

    // F1.4 - SIMULATED kinship verification.
    // There is no real check here, so a valid request cannot fail.
    //
    // IMPORTANT: this is self-service on purpose (the guardian verifies their own
    // link). When real verification arrives (admin review / documents), this
    // endpoint must be removed or restricted, otherwise "Verified" means nothing.
    public async Task<VerifyKinshipResult> VerifyAsync(
        int guardianUserId, int dependentUserId)
    {
        var link = await _context.GuardianPatients
            .FirstOrDefaultAsync(gp =>
                gp.GuardianId == guardianUserId &&
                gp.PatientId == dependentUserId);

        // Missing link and someone else's dependent get the same answer,
        // so ids cannot be probed.
        if (link == null)
        {
            return new VerifyKinshipResult(VerifyKinshipStatus.NotFound);
        }

        if (link.VerificationStatus == VerificationStatus.Verified)
        {
            return new VerifyKinshipResult(
                VerifyKinshipStatus.AlreadyVerified, link.VerifiedAt);
        }

        // Nothing sets Rejected yet; this keeps a future rejection from being
        // bypassed through this simulated endpoint.
        if (link.VerificationStatus == VerificationStatus.Rejected)
        {
            return new VerifyKinshipResult(VerifyKinshipStatus.NotAllowed);
        }

        link.VerificationStatus = VerificationStatus.Verified;
        link.VerifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new VerifyKinshipResult(
            VerifyKinshipStatus.Verified, link.VerifiedAt);
    }
}