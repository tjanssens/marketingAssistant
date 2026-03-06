namespace MarketingAssistant.Discord;

public class DiscordOptions
{
    public const string SectionName = "Discord";

    public string BotToken { get; set; } = string.Empty;
    public ulong GuildId { get; set; }
    public ulong BriefingChannelId { get; set; }
    public ulong AlertsChannelId { get; set; }
    public ulong ContentChannelId { get; set; }
    public ulong ActionsChannelId { get; set; }
}
