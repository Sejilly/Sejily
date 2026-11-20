using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sejily.API.Models.Entities;

namespace Sejily.API.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(x => x.AuditId);

        builder.Property(x => x.Action)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(x => x.EntityType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.OccurredAt).IsRequired();
        builder.Property(x => x.Details); // JSON text

        builder.HasOne(x => x.Request).WithMany()
            .HasForeignKey(x => x.RequestId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Access).WithMany()
            .HasForeignKey(x => x.AccessId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ActorUser).WithMany()
            .HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SubjectPatient).WithMany()
            .HasForeignKey(x => x.SubjectPatientId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Workplace).WithMany()
            .HasForeignKey(x => x.WorkplaceId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.RequestId, x.OccurredAt });
        builder.HasIndex(x => new { x.SubjectPatientId, x.OccurredAt });
    }
}
