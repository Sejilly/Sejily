
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sejily.API.Models.Entities;

namespace Sejily.API.Data.Configurations;

public class EmergencyReviewConfiguration
    : IEntityTypeConfiguration<EmergencyReview>
{
    public void Configure(EntityTypeBuilder<EmergencyReview> builder)
    {
        builder.ToTable("EmergencyReviews", table =>
        {
            table.HasCheckConstraint(
                "CK_EmergencyReviews_Response_RespondedAt",
                """
                (PatientResponse IS NULL AND RespondedAt IS NULL)
                OR
                (PatientResponse IS NOT NULL AND RespondedAt IS NOT NULL)
                """);
        });

        // AccessId is both PK and FK
        builder.HasKey(x => x.AccessId);

        builder.Property(x => x.PatientResponse)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.DeclarationVersion)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.DeclarationText)
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        // One EmergencyReview per Access
        builder.HasOne(x => x.Access)
            .WithOne(x => x.EmergencyReview)
            .HasForeignKey<EmergencyReview>(x => x.AccessId)
            .OnDelete(DeleteBehavior.Restrict);

        // User who responded to the emergency
        builder.HasOne(x => x.RespondedByUser)
            .WithMany()
            .HasForeignKey(x => x.RespondedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
