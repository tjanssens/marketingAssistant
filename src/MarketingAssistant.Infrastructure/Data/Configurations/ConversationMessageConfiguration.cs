using MarketingAssistant.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingAssistant.Infrastructure.Data.Configurations;

public class ConversationMessageConfiguration : IEntityTypeConfiguration<ConversationMessage>
{
    public void Configure(EntityTypeBuilder<ConversationMessage> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Source).HasMaxLength(50).IsRequired();
        builder.Property(c => c.UserId).HasMaxLength(200).IsRequired();
        builder.Property(c => c.UserMessage).IsRequired();
        builder.Property(c => c.AiResponse).IsRequired();
        builder.HasIndex(c => c.Timestamp);
    }
}
