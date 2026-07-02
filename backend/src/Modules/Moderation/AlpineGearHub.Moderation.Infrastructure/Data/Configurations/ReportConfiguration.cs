using AlpineGearHub.Moderation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlpineGearHub.Moderation.Infrastructure.Data.Configurations;

internal sealed class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("reports");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.ListingId).HasColumnName("listing_id");
        builder.Property(r => r.ReportedByUserId).HasColumnName("reported_by_user_id");
        builder.Property(r => r.Reason).HasColumnName("reason").HasConversion<string>().HasMaxLength(50);
        builder.Property(r => r.Description).HasColumnName("description").HasMaxLength(1000).IsRequired(false);
        builder.Property(r => r.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(50);
        builder.Property(r => r.ReviewedByUserId).HasColumnName("reviewed_by_user_id").IsRequired(false);
        builder.Property(r => r.ReviewedAt).HasColumnName("reviewed_at").IsRequired(false);
        builder.Property(r => r.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(r => r.ListingId).HasDatabaseName("ix_reports_listing_id");
        builder.HasIndex(r => r.Status).HasDatabaseName("ix_reports_status");
    }
}
