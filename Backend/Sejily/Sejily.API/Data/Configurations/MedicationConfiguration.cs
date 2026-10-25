using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sejily.API.Models.Entities;

namespace Sejily.API.Data.Configurations;

public class MedicationConfiguration : IEntityTypeConfiguration<Medication>
{
    public void Configure(EntityTypeBuilder<Medication> builder)
    {
        
        builder.HasKey(medication => medication.MedicationId);
        
        builder.HasIndex(medication => medication.ActiveIngredient).IsUnique(false);

    }
}