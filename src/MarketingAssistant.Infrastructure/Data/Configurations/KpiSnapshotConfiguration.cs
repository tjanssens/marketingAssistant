using MarketingAssistant.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingAssistant.Infrastructure.Data.Configurations;

public class KpiSnapshotConfiguration : IEntityTypeConfiguration<KpiSnapshot>
{
    public void Configure(EntityTypeBuilder<KpiSnapshot> builder)
    {
        builder.HasKey(k => k.Id);
        builder.Property(k => k.Revenue).HasColumnType("decimal(18,2)");
        builder.Property(k => k.ConversionRate).HasColumnType("decimal(5,2)");
        builder.Property(k => k.AdSpend).HasColumnType("decimal(18,2)");
        builder.Property(k => k.Roas).HasColumnType("decimal(8,2)");
        builder.HasIndex(k => k.CapturedAt);
    }
}
