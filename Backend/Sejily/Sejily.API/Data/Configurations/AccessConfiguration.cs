
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sejily.API.Models.Entities;

namespace Sejily.API.Data.Configurations;

public class AccessConfiguration
    : IEntityTypeConfiguration<Access>
{
    public void Configure(EntityTypeBuilder<Access> builder)
    {
        builder.ToTable("Accesses", table =>
        {
            table.HasCheckConstraint(
                "CK_Accesses_Permanent_Expiry",
                """
                (ValidityType = 'Permanent' AND ExpiresAt IS NULL)
                OR
                (ValidityType <> 'Permanent' AND ExpiresAt IS NOT NULL)
                """);

            table.HasCheckConstraint(
                "CK_Accesses_Expiry_After_Start",
                """
                ExpiresAt IS NULL OR ExpiresAt > StartAt
                """);
        });

        builder.HasKey(x => x.AccessId);

        builder.Property(x => x.ValidityType)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.StartAt)
            .IsRequired();

        builder.Property(x => x.FirstExaminationDate)
            .HasColumnType("date");

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        // One Access per AccessRequest
        builder.HasOne(x => x.Request)
            .WithOne(x => x.Access)
            .HasForeignKey<Access>(x => x.RequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.RequestId)
            .IsUnique();

        // User who manually closed the access
        builder.HasOne(x => x.ClosedByUser)
            .WithMany()
            .HasForeignKey(x => x.ClosedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
