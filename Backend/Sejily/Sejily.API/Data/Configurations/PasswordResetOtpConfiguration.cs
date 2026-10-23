using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sejily.API.Models.Entities;

namespace Sejily.API.Data.Configurations;

public class PasswordResetOtpConfiguration : IEntityTypeConfiguration<PasswordResetOtp>
{
    public void Configure(EntityTypeBuilder<PasswordResetOtp> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OtpHash).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ResetTokenHash).HasMaxLength(64);
    }
}
