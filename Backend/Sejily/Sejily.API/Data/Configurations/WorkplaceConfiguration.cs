using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sejily.API.Models.Entities;

namespace Sejily.API.Data.Configurations;

public class WorkplaceConfiguration : IEntityTypeConfiguration<Workplace>
{
    public void Configure(EntityTypeBuilder<Workplace> builder)
    {
        builder.HasKey(w => w.WorkplaceId);
        
        builder.Property(w => w.Name).HasMaxLength(200).IsRequired();
        
        builder.Property(w => w.Type).HasMaxLength(150).IsRequired();
        
        builder.Property(w => w.Address).HasMaxLength(500).IsRequired();
        
        builder.Property(w => w.LicenseNumber).HasMaxLength(100).IsRequired();
    }
}
