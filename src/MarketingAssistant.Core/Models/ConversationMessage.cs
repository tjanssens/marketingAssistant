namespace MarketingAssistant.Core.Models;

public class ConversationMessage
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Source { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserMessage { get; set; } = string.Empty;
    public string AiResponse { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}
