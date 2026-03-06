using MarketingAssistant.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingAssistant.Infrastructure.Data.Configurations;

public class BriefingConfiguration : IEntityTypeConfiguration<Briefing>
{
    public void Configure(EntityTypeBuilder<Briefing> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Title).HasMaxLength(500).IsRequired();
        builder.Property(b => b.Content).IsRequired();
        builder.Property(b => b.Period).HasMaxLength(100);
        builder.HasMany(b => b.Actions).WithOne(a => a.Briefing).HasForeignKey(a => a.BriefingId);
        builder.HasIndex(b => b.GeneratedAt);
    }
}
