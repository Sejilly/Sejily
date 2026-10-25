using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sejily.API.Models.Entities;

namespace Sejily.API.Data.Configurations;


public class MedicationUsageConfiguration : IEntityTypeConfiguration<MedicationUsage>
{
    public void Configure(EntityTypeBuilder<MedicationUsage> builder)
    {
        builder.HasKey(usage => usage.MedicationUsageId);
        
        builder.OwnsOne(usage => usage.Frequency);
        builder.OwnsOne(usage => usage.Duration);
        builder.OwnsOne(usage => usage.Dose, doseBuilder =>
        {
            doseBuilder.Property(dose => dose.Amount).HasColumnType("DECIMAL(18, 4)");
        });

        builder.HasOne(usage => usage.Medication)
            .WithMany()
            .HasForeignKey(usage => usage.MedicationId);
        
        builder.HasOne(usage => usage.MedicalEncounter)
            .WithMany(encounter => encounter.Medications)
            .HasForeignKey(usage => usage.MedicationId);
        
    }
}