
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sejily.API.Models.Entities;
using Sejily.API.Models.Enums;

namespace Sejily.API.Data.Configurations;

public class AccessRequestConfiguration: IEntityTypeConfiguration<AccessRequest>
{
    public void Configure(EntityTypeBuilder<AccessRequest> builder)
    {
        builder.ToTable("AccessRequests", table =>
        {
            table.HasCheckConstraint(
                "CK_AccessRequests_Decision_DecidedAt",
                """
                (Decision IS NULL AND DecidedAt IS NULL)
                OR
                (Decision IS NOT NULL AND DecidedAt IS NOT NULL)
                """);
        });

        builder.HasKey(x => x.RequestId);

        builder.Property(x => x.AccessType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Decision)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.SentAt)
            .IsRequired();

        builder.Property(x => x.RejectionRequestedAt);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        // Doctor relationship: Doctor.UserId
        builder.HasOne(x => x.Doctor)
            .WithMany()
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Patient relationship: Patient.UserId
        builder.HasOne(x => x.RecipientPatient)
            .WithMany()
            .HasForeignKey(x => x.RecipientPatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Workplace relationship
        builder.HasOne(x => x.Workplace)
            .WithMany()
            .HasForeignKey(x => x.WorkplaceId)
            .OnDelete(DeleteBehavior.Restrict);

        // User who responded
        builder.HasOne(x => x.RespondedByUser)
            .WithMany()
            .HasForeignKey(x => x.RespondedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Self-reference: request replaced by another request
        builder.HasOne(x => x.SupersededByRequest)
            .WithMany(x => x.SupersededRequests)
            .HasForeignKey(x => x.SupersededByRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        // Useful indexes for doctor and patient lists
        builder.HasIndex(x => new { x.DoctorId, x.SentAt });

        builder.HasIndex(x => new { x.RecipientPatientId, x.SentAt });

        // Used when checking duplicate requests
        builder.HasIndex(x => new
        {
            x.DoctorId,
            x.WorkplaceId,
            x.RecipientPatientId
        });
    }
}
