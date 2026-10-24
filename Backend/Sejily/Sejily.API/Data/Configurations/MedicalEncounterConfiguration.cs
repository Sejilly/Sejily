using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Sejily.API.Models.Entities;

namespace Sejily.API.Data.Configurations;


public class MedicalEncounterConfiguration : IEntityTypeConfiguration<MedicalEncounter>
{
    public void Configure(EntityTypeBuilder<MedicalEncounter> builder)
    {
        builder.HasKey(encounter => encounter.MedicalEncounterId);

        builder.HasAlternateKey(encounter => new
        {
            encounter.PatientId,
            encounter.DateTime
        });

        builder.HasAlternateKey(encounter => new
        {
            encounter.DoctorId,
            encounter.DateTime
        });
    }
}