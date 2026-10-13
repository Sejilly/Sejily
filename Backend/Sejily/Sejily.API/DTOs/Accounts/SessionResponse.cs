namespace Sejily.API.DTOs.Accounts;

public class SessionResponse
{
    // The account requests are acting as right now (always show its name in the UI).
    public AccountInfo ActiveAccount { get; set; } = null!;

    // The person who actually logged in (the guardian).
    public AccountInfo LoggedInAs { get; set; } = null!;

    // Show the "Account Switching" option only when this is true.
    public bool CanSwitch { get; set; }

    // Own account first, then verified dependents.
    public List<AccountInfo> Accounts { get; set; } = new();
}
