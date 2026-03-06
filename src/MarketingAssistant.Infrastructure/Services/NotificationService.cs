using MarketingAssistant.Core.DTOs;
using MarketingAssistant.Core.Interfaces;
using MarketingAssistant.Core.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace MarketingAssistant.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<DashboardHubMarker> _hub;
    private readonly IDiscordNotifier? _discord;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IHubContext<DashboardHubMarker> hub, ILogger<NotificationService> logger, IEnumerable<IDiscordNotifier> discordNotifiers)
    {
        _hub = hub;
        _logger = logger;
        _discord = discordNotifiers.FirstOrDefault();
    }

    public async Task SendBriefingAsync(Briefing briefing, CancellationToken ct = default)
    {
        var dto = new BriefingSummaryDto(
            briefing.Id, briefing.GeneratedAt, briefing.Title, briefing.Period, briefing.Actions.Count
        );
        await _hub.Clients.All.SendAsync("NewBriefing", dto, ct);
        _logger.LogInformation("SignalR: NewBriefing sent for {Title}", briefing.Title);

        if (_discord is not null)
        {
            try
            {
                await _discord.SendBriefingAsync(briefing, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send briefing to Discord");
            }
        }
    }

    public async Task SendAlertAsync(Alert alert, CancellationToken ct = default)
    {
        var dto = new AlertDto(
            alert.Id, alert.CreatedAt, alert.Severity, alert.Title, alert.Message, alert.Category, alert.IsAcknowledged
        );
        await _hub.Clients.All.SendAsync("NewAlert", dto, ct);
        _logger.LogInformation("SignalR: NewAlert sent for {Title}", alert.Title);

        if (_discord is not null)
        {
            try
            {
                await _discord.SendAlertAsync(alert, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send alert to Discord");
            }
        }
    }

    public async Task SendActionUpdateAsync(ActionItem action, CancellationToken ct = default)
    {
        var dto = new ActionItemDto(
            action.Id, action.BriefingId, action.Description, action.Type, action.Status,
            action.SuggestedAt, action.ResolvedAt, action.ResolvedBy, action.AiReasoning
        );
        await _hub.Clients.All.SendAsync("ActionUpdated", dto, ct);
        _logger.LogInformation("SignalR: ActionUpdated sent for {Id} -> {Status}", action.Id, action.Status);
    }
}
