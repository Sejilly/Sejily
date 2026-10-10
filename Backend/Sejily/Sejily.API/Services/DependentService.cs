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

    public DependentService(
        UserManager<User> userManager,
        SejilyDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    // =========================
    // F1.3 - Add a dependent
    // =========================

    public async Task<AddDependentResult> AddDependentAsync(
        int guardianUserId, AddDependentRequest request)
    {
        // 1) Dependency on F1.1: the guardian must already be a registered patient
        var guardianIsPatient = await _context.Patients
            .AnyAsync(p => p.UserId == guardianUserId);

        if (!guardianIsPatient)
        {
            return new AddDependentResult(
                AddDependentStatus.GuardianNotFound,
                Errors: new[] { "Only a registered patient can add a dependent." });
        }

        // 2) Duplicates -> a clean 409 instead of a database error
        if (await _userManager.FindByNameAsync(BuildUserName(request.NationalID)) != null)
        {
            return new AddDependentResult(
                AddDependentStatus.Conflict,
                Errors: new[] { "National ID already exists." });
        }

        if (await _context.Patients.AnyAsync(
                p => p.HealthCardNumber == request.HealthCardNumber))
        {
            return new AddDependentResult(
                AddDependentStatus.Conflict,
                Errors: new[] { "Health card number already exists." });
        }

        // 3) All-or-nothing: User + Patient + GuardianPatient are saved together.
        //    UserManager.CreateAsync uses the same DbContext, so it joins this transaction.
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // No password: the dependent cannot log in directly.
            var user = new User
            {
                UserName = BuildUserName(request.NationalID),
                NationalID = request.NationalID,
                FullName = request.FullName,
                Address = request.Address,
                DateOfBirth = request.DateOfBirth,
                BloodType = request.BloodType,
                PhoneNumber = request.PhoneNumber
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
                HealthCardNumber = request.HealthCardNumber
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
    // F1.3 - Dependents shown in the guardian's account
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
    // F1.1 currently uses the National ID directly. When the separate
    // doctor/patient accounts change lands, only this method needs to change.
    private static string BuildUserName(string nationalId) => nationalId;
}