using MarketingAssistant.Core.Enums;

namespace MarketingAssistant.Core.Models;

public class Briefing
{
    public int Id { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string RawData { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public List<ActionItem> Actions { get; set; } = [];
}
