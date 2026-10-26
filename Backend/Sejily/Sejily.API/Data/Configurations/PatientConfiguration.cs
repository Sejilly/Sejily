using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sejily.API.Models.Entities;

namespace Sejily.API.Data.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        // Family history uses the same PatientDisease rows and IsFamilyDisease flag.
        // Avoid mapping a second collection relationship over PatientDisease.Patient.
        builder.Ignore(p => p.FamilyDiseases);

        builder.HasKey(p => p.UserId);
        
        builder.Property(p => p.HealthCardNumber).HasMaxLength(50);
        
        builder.HasIndex(p => p.HealthCardNumber).IsUnique();
        
        builder.HasOne(p => p.User)
            .WithOne(u => u.Patient)
            .HasForeignKey<Patient>(p => p.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
