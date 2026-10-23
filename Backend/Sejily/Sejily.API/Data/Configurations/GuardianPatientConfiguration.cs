using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sejily.API.Models.Entities;

namespace Sejily.API.Data.Configurations;

public class GuardianPatientConfiguration : IEntityTypeConfiguration<GuardianPatient>
{
    public void Configure(EntityTypeBuilder<GuardianPatient> builder)
    {
        builder.HasKey(gp => new { gp.GuardianId, gp.PatientId });
        builder.HasOne(gp => gp.Guardian).WithMany(p => p.Dependents)
            .HasForeignKey(gp => gp.GuardianId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(gp => gp.Patient).WithMany(p => p.Guardians)
            .HasForeignKey(gp => gp.PatientId).OnDelete(DeleteBehavior.NoAction);
    }
}
