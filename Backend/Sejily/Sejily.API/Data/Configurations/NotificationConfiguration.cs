using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sejily.API.Models.Entities;

namespace Sejily.API.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(x => x.NotificationId);

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.IsRead).HasDefaultValue(false);

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

        // Unread counter
        builder.HasIndex(x => new { x.RecipientUserId, x.IsRead });

        // Cursor pagination (newest first) for the list endpoint
        builder.HasIndex(x => new { x.RecipientUserId, x.CreatedAt, x.NotificationId })
            .HasFilter("[DeletedAt] IS NULL");
    }
}
