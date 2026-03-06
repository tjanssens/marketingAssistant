using MarketingAssistant.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingAssistant.Infrastructure.Data.Configurations;

public class ActionItemConfiguration : IEntityTypeConfiguration<ActionItem>
{
    public void Configure(EntityTypeBuilder<ActionItem> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Description).HasMaxLength(1000).IsRequired();
        builder.Property(a => a.AiReasoning).HasMaxLength(2000);
        builder.Property(a => a.Parameters).HasDefaultValue("{}");
        builder.Property(a => a.ResolvedBy).HasMaxLength(200);
        builder.Property(a => a.DiscordMessageId).HasMaxLength(100);
        builder.Property(a => a.Type).HasConversion<string>().HasMaxLength(50);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(50);
        builder.HasIndex(a => a.Status);
    }
}
