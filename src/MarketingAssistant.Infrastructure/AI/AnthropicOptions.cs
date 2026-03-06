namespace MarketingAssistant.Infrastructure.AI;

public class AnthropicOptions
{
    public const string SectionName = "Anthropic";
    public string BaseUrl { get; set; } = "https://api.anthropic.com";
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "claude-sonnet-4-20250514";
    public int MaxTokens { get; set; } = 4096;
}
