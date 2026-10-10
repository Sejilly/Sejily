namespace Sejily.API.Models.Enums
{
    public enum VerifyKinshipStatus
    {
        Verified,
        AlreadyVerified,   // repeated call: still a success, nothing changes
        NotFound,          // no such dependent for this guardian
        NotAllowed         // the link was rejected, so it cannot be verified here
    }
}
