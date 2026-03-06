using MarketingAssistant.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingAssistant.Infrastructure.Data.Configurations;

public class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Title).HasMaxLength(500).IsRequired();
        builder.Property(a => a.Message).HasMaxLength(2000).IsRequired();
        builder.Property(a => a.Category).HasMaxLength(100);
        builder.Property(a => a.DiscordMessageId).HasMaxLength(100);
        builder.Property(a => a.Severity).HasConversion<string>().HasMaxLength(50);
        builder.HasIndex(a => a.CreatedAt);
        builder.HasIndex(a => a.IsAcknowledged);
    }
}
