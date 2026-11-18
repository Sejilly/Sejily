using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sejily.API.Models.Entities;

namespace Sejily.API.Data.Configurations;


public class PatientFamilyDiseaseConfiguration : IEntityTypeConfiguration<PatientFamilyDisease>
{
    public void Configure(EntityTypeBuilder<PatientFamilyDisease> builder)
    {
        builder.HasOne(familyDisease => familyDisease.Disease)
            .WithMany()
            .HasForeignKey(familyDisease => familyDisease.DiseaseId);
        
        builder.HasOne(familyDisease => familyDisease.Patient)
            .WithMany(patient => patient.FamilyDiseases)
            .HasForeignKey(familyDisease => familyDisease.PatientId);
        
        builder.HasKey(familyDisease => new
        {
            familyDisease.PatientId,
            familyDisease.DiseaseId
        });
    }
}