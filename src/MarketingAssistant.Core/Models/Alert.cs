using MarketingAssistant.Core.Enums;

namespace MarketingAssistant.Core.Models;

public class Alert
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public AlertSeverity Severity { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsAcknowledged { get; set; }
    public string? DiscordMessageId { get; set; }
}
