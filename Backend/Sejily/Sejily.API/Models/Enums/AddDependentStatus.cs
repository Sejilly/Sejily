namespace Sejily.API.Models.Enums
{
    public enum AddDependentStatus
    {
        Created,
        GuardianNotFound,   // the caller has no Patient profile (F1.1 dependency)
        Conflict,           // National ID or Health Card Number already exists
        Invalid             // rejected by ASP.NET Core Identity
    }
}
