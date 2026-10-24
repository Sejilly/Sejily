using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sejily.API.Models.Entities;

namespace Sejily.API.Data.Configurations;


public class PatientDiseaseConfiguration : IEntityTypeConfiguration<PatientDisease>
{
    public void Configure(EntityTypeBuilder<PatientDisease> builder)
    {

        builder.HasKey(patientDisease => patientDisease.PatientDiseaseId);

        builder.HasIndex(patientDisease => new
        {
            
            patientDisease.PatientId,
            patientDisease.DiseaseId,
            patientDisease.MedicalEncounterId

        }).IsUnique();

        builder.HasOne(patientDisease => patientDisease.Patient)
            .WithMany(patient => patient.Diseases)
            .HasForeignKey(patientDisease => patientDisease.PatientId);

        builder.HasOne(patientDisease => patientDisease.Disease)
            .WithMany()
            .HasForeignKey(patientDisease => patientDisease.DiseaseId);

        builder.HasOne(patientDisease => patientDisease.MedicalEncounter)
            .WithMany(encounter => encounter.Diseases)
            .HasForeignKey(patientDisease => patientDisease.MedicalEncounterId);


    }
}