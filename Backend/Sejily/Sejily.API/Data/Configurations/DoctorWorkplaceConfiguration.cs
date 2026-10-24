using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sejily.API.Models.Entities;

namespace Sejily.API.Data.Configurations;

public class DoctorWorkplaceConfiguration : IEntityTypeConfiguration<DoctorWorkplace>
{
    public void Configure(EntityTypeBuilder<DoctorWorkplace> builder)
    {
        builder.HasKey(dw => new { dw.DoctorId, dw.WorkplaceId });
        
        builder.HasOne(dw => dw.Doctor)
            .WithMany(d => d.Workplaces)
            .HasForeignKey(dw => dw.DoctorId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasOne(dw => dw.Workplace)
            .WithMany(w => w.Doctors)
            .HasForeignKey(dw => dw.WorkplaceId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
