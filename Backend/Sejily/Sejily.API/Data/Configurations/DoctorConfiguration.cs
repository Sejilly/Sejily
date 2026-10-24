using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sejily.API.Models.Entities;

namespace Sejily.API.Data.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.HasKey(doctor => doctor.UserId);
        
        builder.Property(d => d.SyndicateCardNumber)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(d => d.MedicalLicenseNumber)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(d => d.Specialization)
            .HasMaxLength(150)
            .IsRequired();
        
        builder.HasIndex(d => d.SyndicateCardNumber)
            .IsUnique();
        
        builder.HasIndex(d => d.MedicalLicenseNumber)
            .IsUnique();
        
        builder.HasOne(d => d.User)
            .WithOne(u => u.Doctor)
            .HasForeignKey<Doctor>(d => d.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
