using Discord;
using Discord.WebSocket;
using MarketingAssistant.Core.Interfaces;
using MarketingAssistant.Core.Models;
using MarketingAssistant.Discord.Builders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MarketingAssistant.Discord;

public class DiscordNotifier : IDiscordNotifier
{
    private readonly DiscordSocketClient _client;
    private readonly DiscordOptions _options;
    private readonly ILogger<DiscordNotifier> _logger;

    public DiscordNotifier(DiscordSocketClient client, IOptions<DiscordOptions> options, ILogger<DiscordNotifier> logger)
    {
        _client = client;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendBriefingAsync(Briefing briefing, CancellationToken ct = default)
    {
        var channel = GetChannel(_options.BriefingChannelId);
        if (channel is null) return;

        var embed = EmbedBuilders.BuildBriefingEmbed(briefing);
        await channel.SendMessageAsync(embed: embed);

        // Send action items with buttons
        foreach (var action in briefing.Actions)
        {
            var actionEmbed = EmbedBuilders.BuildActionEmbed(action);
            var buttons = EmbedBuilders.BuildActionButtons(action.Id);
            await channel.SendMessageAsync(embed: actionEmbed, components: buttons.Build());
        }

        _logger.LogInformation("Discord: Briefing sent to #{Channel}", channel.Name);
    }

    public async Task SendAlertAsync(Alert alert, CancellationToken ct = default)
    {
        var channel = GetChannel(_options.AlertsChannelId);
        if (channel is null) return;

        var embed = EmbedBuilders.BuildAlertEmbed(alert);
        await channel.SendMessageAsync(embed: embed);

        _logger.LogInformation("Discord: Alert sent to #{Channel}", channel.Name);
    }

    public async Task SendActionWithButtonsAsync(ActionItem action, CancellationToken ct = default)
    {
        var channel = GetChannel(_options.ActionsChannelId);
        if (channel is null) return;

        var embed = EmbedBuilders.BuildActionEmbed(action);
        var buttons = EmbedBuilders.BuildActionButtons(action.Id);
        await channel.SendMessageAsync(embed: embed, components: buttons.Build());

        _logger.LogInformation("Discord: Action sent to #{Channel}", channel.Name);
    }

    private IMessageChannel? GetChannel(ulong channelId)
    {
        if (channelId == 0)
        {
            _logger.LogDebug("Discord channel ID not configured, skipping notification");
            return null;
        }

        var channel = _client.GetChannel(channelId) as IMessageChannel;
        if (channel is null)
        {
            _logger.LogWarning("Discord channel {ChannelId} not found or not a text channel", channelId);
        }

        return channel;
    }
}
