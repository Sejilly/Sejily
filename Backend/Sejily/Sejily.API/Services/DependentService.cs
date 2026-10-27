using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sejily.API.Data;
using Sejily.API.DTOs.Dependents;
using Sejily.API.Models.Entities;
using Sejily.API.Models.Enums;

namespace Sejily.API.Services;

public class DependentService : IDependentService
{
    private readonly UserManager<User> _userManager;
    private readonly SejilyDbContext _context;
    private readonly IPatientHealthService _healthService;

    public DependentService(
        UserManager<User> userManager,
        SejilyDbContext context,
        IPatientHealthService healthService)
    {
        _userManager = userManager;
        _context = context;
        _healthService = healthService;
    }

    // =========================
    // Add a dependent
    // =========================

    public async Task<AddDependentResult> AddDependentAsync(
        int guardianUserId, AddDependentRequest request)
    {
        // the guardian must already be a registered patient
        var guardianIsPatient = await _context.Patients
            .AnyAsync(p => p.UserId == guardianUserId);

        if (!guardianIsPatient)
        {
            return new AddDependentResult(
                AddDependentStatus.GuardianNotFound,
                Errors: new[] { "Only a registered patient can add a dependent." });
        }

        var healthCard = string.IsNullOrWhiteSpace(request.HealthCardNumber)
    ? null
    : request.HealthCardNumber.Trim();

        // 2) The depenfent must not be registered before (a Duplicate)
        if (await _context.Users.AnyAsync(u =>
                u.NationalID == request.NationalID &&
                u.AccountType == AccountType.Patient))
        {
            return new AddDependentResult(
                AddDependentStatus.Conflict,
                Errors: new[] { "National ID already exists." });
        }

        if (healthCard != null &&
        await _context.Patients.AnyAsync(p => p.HealthCardNumber == healthCard))
        {
            return new AddDependentResult(
                AddDependentStatus.Conflict,
                Errors: new[] { "Health card number already exists." });
        }

        // 3) All-or-nothing: User + Patient + GuardianPatient are saved together.
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // No password: the dependent cannot log in directly.
            var user = new User
            {
                UserName = BuildUserName(request.NationalID),
                NationalID = request.NationalID,
                AccountType = AccountType.Patient,
                FullName = request.FullName,
                Address = request.Address,
                DateOfBirth = request.DateOfBirth,
                //PhoneNumber = request.PhoneNumber
            };

            var identityResult = await _userManager.CreateAsync(user);

            if (!identityResult.Succeeded)
            {
                return new AddDependentResult(
                    AddDependentStatus.Invalid,
                    Errors: identityResult.Errors.Select(e => e.Description));
            }

            _context.Patients.Add(new Patient
            {
                UserId = user.Id,
                //BloodType = request.BloodType,
                HealthCardNumber = healthCard
            });

            // Kinship is verified later (F1.4), so the link starts as Pending.
            _context.GuardianPatients.Add(new GuardianPatient
            {
                GuardianId = guardianUserId,
                PatientId = user.Id,
                Relationship = request.Relationship,
                VerificationStatus = VerificationStatus.Pending
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new AddDependentResult(AddDependentStatus.Created, user.Id);
        }
        catch (DbUpdateException)
        {
            // Unique-index violation from a concurrent request; the transaction rolls back.
            return new AddDependentResult(
                AddDependentStatus.Conflict,
                Errors: new[] { "National ID or health card number already exists." });
        }
    }

    // ======================================
    // Two-step registration (mobile wizard)
    // ======================================

    private static readonly TimeSpan DraftLifetime = TimeSpan.FromMinutes(30);

    // Step 1: validate the data screen right away (duplicate National ID /
    // health card) and keep it as a draft bound to the guardian.
    public async Task<StartDependentRegistrationResult> StartRegistrationAsync(
        int guardianUserId, StartDependentRegistrationRequest request)
    {
        if (!await _context.Patients.AnyAsync(p => p.UserId == guardianUserId))
        {
            return new StartDependentRegistrationResult(
                AddDependentStatus.GuardianNotFound,
                Errors: new[] { "Only a registered patient can add a dependent." });
        }

        var nationalId = request.NationalID.Trim();
        var healthCard = NormalizeHealthCard(request.HealthCardNumber);

        var duplicate = await FindDuplicateAsync(nationalId, healthCard);
        if (duplicate != null)
        {
            return new StartDependentRegistrationResult(
                AddDependentStatus.Conflict, Errors: new[] { duplicate });
        }

        var draft = new DependentRegistrationDraft
        {
            RequestId = Guid.NewGuid(),
            GuardianUserId = guardianUserId,
            FullName = request.FullName.Trim(),
            NationalID = nationalId,
            Address = request.Address.Trim(),
            DateOfBirth = request.DateOfBirth,
            HealthCardNumber = healthCard,
            ExpiresAtUtc = DateTime.UtcNow.Add(DraftLifetime)
        };

        _context.DependentRegistrationDrafts.Add(draft);
        await _context.SaveChangesAsync();

        return new StartDependentRegistrationResult(
            AddDependentStatus.Created, draft.RequestId);
    }

    // Step 2: User + Patient + GuardianPatient are saved together or not at all.
    public async Task<AddDependentResult> CompleteRegistrationAsync(
        int guardianUserId, CompleteDependentRegistrationRequest request)
    {
        if (!Enum.IsDefined(request.Relationship))
        {
            return new AddDependentResult(
                AddDependentStatus.Invalid,
                Errors: new[] { "Invalid relationship." });
        }

        var now = DateTime.UtcNow;

        // Same answer for "missing", "expired" and "someone else's" draft.
        var draft = await _context.DependentRegistrationDrafts
            .FirstOrDefaultAsync(d =>
                d.RequestId == request.RequestId &&
                d.GuardianUserId == guardianUserId &&
                d.ExpiresAtUtc > now);

        if (draft == null)
        {
            return new AddDependentResult(
                AddDependentStatus.DraftNotFound,
                Errors: new[] { "Invalid or expired registration request." });
        }

        if (!await _context.Patients.AnyAsync(p => p.UserId == guardianUserId))
        {
            return new AddDependentResult(
                AddDependentStatus.GuardianNotFound,
                Errors: new[] { "Only a registered patient can add a dependent." });
        }

        // Re-check: someone may have registered the same person since step 1.
        var duplicate = await FindDuplicateAsync(draft.NationalID, draft.HealthCardNumber);
        if (duplicate != null)
        {
            return new AddDependentResult(
                AddDependentStatus.Conflict, Errors: new[] { duplicate });
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var user = new User
            {
                UserName = BuildUserName(draft.NationalID),
                NationalID = draft.NationalID,
                AccountType = AccountType.Patient,
                FullName = draft.FullName,
                Address = draft.Address,
                DateOfBirth = draft.DateOfBirth
            };

            var identityResult = await _userManager.CreateAsync(user);

            if (!identityResult.Succeeded)
            {
                await transaction.RollbackAsync();

                return new AddDependentResult(
                    AddDependentStatus.Invalid,
                    Errors: identityResult.Errors.Select(e => e.Description));
            }

            var patient = new Patient
            {
                UserId = user.Id,
                HealthCardNumber = draft.HealthCardNumber
            };

            _context.Patients.Add(patient);

            // Kinship is verified later (F1.4), so the link starts as Pending.
            _context.GuardianPatients.Add(new GuardianPatient
            {
                GuardianId = guardianUserId,
                PatientId = user.Id,
                Relationship = request.Relationship,
                VerificationStatus = VerificationStatus.Pending
            });

            _context.DependentRegistrationDrafts.Remove(draft);

            // Token for step 3 (health data), only this guardian can use it.
            var healthRequestId = _healthService.AddSetupRequest(user.Id, guardianUserId);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new AddDependentResult(
                AddDependentStatus.Created, user.Id, HealthRequestId: healthRequestId);
        }
        catch (DbUpdateException)
        {
            // Unique-index violation from a concurrent request; the transaction rolls back.
            return new AddDependentResult(
                AddDependentStatus.Conflict,
                Errors: new[] { "National ID or health card number already exists." });
        }
    }

    private static string? NormalizeHealthCard(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    // Returns an error message when the National ID or health card is taken.
    private async Task<string?> FindDuplicateAsync(string nationalId, string? healthCard)
    {
        if (await _context.Users.AnyAsync(u =>
                u.NationalID == nationalId &&
                u.AccountType == AccountType.Patient))
        {
            return "National ID already exists.";
        }

        if (healthCard != null &&
            await _context.Patients.AnyAsync(p => p.HealthCardNumber == healthCard))
        {
            return "Health card number already exists.";
        }

        return null;
    }

    // ======================================
    // Dependents shown in the guardian's account
    // ======================================

    public async Task<List<DependentResponse>> GetDependentsAsync(int guardianUserId)
    {
        return await _context.GuardianPatients
            .AsNoTracking()
            .Where(gp => gp.GuardianId == guardianUserId)
            .Select(gp => new DependentResponse
            {
                PatientId = gp.PatientId,
                FullName = gp.Patient.User.FullName,
                DateOfBirth = gp.Patient.User.DateOfBirth,
                Relationship = gp.Relationship,
                VerificationStatus = gp.VerificationStatus
            })
            .ToListAsync();
    }

    // The single place that decides the Identity UserName of a patient account.
    private static string BuildUserName(string nationalId) => $"{nationalId}_Patient";
}