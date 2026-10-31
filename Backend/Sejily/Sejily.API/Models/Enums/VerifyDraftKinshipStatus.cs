namespace Sejily.API.Models.Enums;

public enum VerifyDraftKinshipStatus
{
    Verified,
    DraftNotFound,   // registration request is missing, expired or belongs to someone else
    Invalid          // unknown relationship value
}
