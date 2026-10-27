using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sejily.API.Models.Entities;

namespace Sejily.API.Data.Configurations;

public class PatientHealthSetupRequestConfiguration
    : IEntityTypeConfiguration<PatientHealthSetupRequest>
{
    public void Configure(EntityTypeBuilder<PatientHealthSetupRequest> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasIndex(r => r.RequestId).IsUnique();
    }
}
