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

    public DbSet<AccessRequest> AccessRequests { get; set; }

    public DbSet<Access> Accesses { get; set; }

    public DbSet<EmergencyReview> EmergencyReviews { get; set; }

    public DbSet<Notification> Notifications { get; set; }

    public DbSet<DeviceToken> DeviceTokens { get; set; }

    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // All DateTime columns are UTC. Reads come back flagged as UTC so the API emits "Z".
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<NullableUtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SejilyDbContext).Assembly);

        // Birth dates are calendar dates, not moments in time: never treat them as UTC instants.
        foreach (var property in modelBuilder.Model.GetEntityTypes()
                     .SelectMany(e => e.GetProperties())
                     .Where(p => p.Name == "DateOfBirth"))
        {
            property.SetValueConverter((Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter?)null);
        }
    }
}
