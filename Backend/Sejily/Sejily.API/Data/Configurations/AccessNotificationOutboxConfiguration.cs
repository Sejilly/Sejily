using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sejily.API.Models.Entities;

namespace Sejily.API.Data.Configurations;

public sealed class AccessNotificationOutboxConfiguration : IEntityTypeConfiguration<AccessNotificationOutbox>
{
    public void Configure(EntityTypeBuilder<AccessNotificationOutbox> builder)
    {
        builder.ToTable("AccessNotificationOutbox");
        builder.HasKey(x => x.OutboxId);

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(x => x.OccurredAt).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasOne(x => x.RecipientUser)
            .WithMany()
            .HasForeignKey(x => x.RecipientUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.SubjectPatient)
            .WithMany()
            .HasForeignKey(x => x.SubjectPatientId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Request)
            .WithMany()
            .HasForeignKey(x => x.RequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.ProcessedAt, x.OccurredAt });
    }
}
