using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sejily.API.Models.Entities;

namespace Sejily.API.Data.Configurations;

public class DiseaseConfiguration : IEntityTypeConfiguration<Disease>
{
    public void Configure(EntityTypeBuilder<Disease> builder)
    {
        builder.HasKey(disease => disease.DiseaseId);

        builder.HasIndex(disease => new
        {
            disease.IcdVersion,
            disease.IcdCode
        })
        .IsUnique();

        builder.HasAlternateKey(disease => disease.IcdUri);

        builder.Property(disease => disease.Title)
            .HasColumnType("VARCHAR(200)");

        builder.Property(disease => disease.TitleAr)
            .HasColumnType("NVARCHAR(200)");

    }
}