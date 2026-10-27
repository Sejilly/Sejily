using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sejily.API.Models.Entities;

namespace Sejily.API.Data.Configurations;

public class DependentRegistrationDraftConfiguration
    : IEntityTypeConfiguration<DependentRegistrationDraft>
{
    public void Configure(EntityTypeBuilder<DependentRegistrationDraft> builder)
    {
        builder.HasKey(d => d.Id);

        builder.HasIndex(d => d.RequestId).IsUnique();

        builder.Property(d => d.FullName).IsRequired();
        builder.Property(d => d.NationalID).IsRequired();
        builder.Property(d => d.Address).IsRequired();
    }
}
