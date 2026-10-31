using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sejily.API.Models.Entities;

namespace Sejily.API.Data;

public class SejilyDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public SejilyDbContext(
        DbContextOptions<SejilyDbContext> options)
        : base(options)
    {
    }

    // =========================
    // DbSets
    // =========================

    public DbSet<PatientRegistrationDraft> PatientRegistrationDrafts
    => Set<PatientRegistrationDraft>();
    public DbSet<DependentRegistrationDraft> DependentRegistrationDrafts
    => Set<DependentRegistrationDraft>();
    public DbSet<PatientHealthSetupRequest> PatientHealthSetupRequests
    => Set<PatientHealthSetupRequest>();
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Workplace> Workplaces { get; set; }
    public DbSet<GuardianPatient> GuardianPatients { get; set; }

    public DbSet<PasswordResetOtp> PasswordResetOtps { get; set; }

    public DbSet<Disease> Diseases { get; set; }
    public DbSet<MedicalEncounter> MedicalEncounters { get; set; }
    public DbSet<Medication> Medications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SejilyDbContext).Assembly);
    }
}
