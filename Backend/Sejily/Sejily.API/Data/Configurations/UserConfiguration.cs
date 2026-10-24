using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sejily.API.Models.Entities;

namespace Sejily.API.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        
        builder.Property(u => u.NationalID)
            .HasMaxLength(14)
            .IsFixedLength()
            .IsRequired();

        builder.HasIndex(u => new { u.NationalID, u.AccountType }).IsUnique();

        builder.Property(u => u.FullName).HasMaxLength(200).IsRequired();

        builder.Property(u => u.Address).HasMaxLength(500).IsRequired();

    }
}
