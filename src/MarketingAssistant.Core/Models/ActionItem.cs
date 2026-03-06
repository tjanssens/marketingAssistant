using MarketingAssistant.Core.Enums;

namespace MarketingAssistant.Core.Models;

public class ActionItem
{
    public int Id { get; set; }
    public int? BriefingId { get; set; }
    public Briefing? Briefing { get; set; }
    public string Description { get; set; } = string.Empty;
    public ActionType Type { get; set; }
    public ActionStatus Status { get; set; }
    public DateTime SuggestedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedBy { get; set; }
    public string AiReasoning { get; set; } = string.Empty;
    public string Parameters { get; set; } = "{}";
    public string? DiscordMessageId { get; set; }
}
