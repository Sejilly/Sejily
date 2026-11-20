using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Sejily.API.Data;
using Sejily.API.Extensions;

namespace Sejily.API.Authorization;

// Policy "VerifiedDoctor": the caller is a doctor AND Doctor.IsVerified is true in the DB.
// Checked against the DB (not a token claim) so verification changes apply immediately.
public class VerifiedDoctorRequirement : IAuthorizationRequirement { }

public class VerifiedDoctorHandler : AuthorizationHandler<VerifiedDoctorRequirement>
{
    private readonly SejilyDbContext _db;

    public VerifiedDoctorHandler(SejilyDbContext db) => _db = db;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, VerifiedDoctorRequirement requirement)
    {
        if (!context.User.HasClaim("AccountType", "Doctor")) return;

        var id = context.User.GetActiveUserId();
        if (id is null) return;

        if (await _db.Doctors.AnyAsync(d => d.UserId == id && d.IsVerified))
            context.Succeed(requirement);
    }
}
