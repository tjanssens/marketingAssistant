using MarketingAssistant.Core.DTOs;
using MarketingAssistant.Core.Interfaces;
using MarketingAssistant.Core.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace MarketingAssistant.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<DashboardHubMarker> _hub;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IHubContext<DashboardHubMarker> hub, ILogger<NotificationService> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    public async Task SendBriefingAsync(Briefing briefing, CancellationToken ct = default)
    {
        var dto = new BriefingSummaryDto(
            briefing.Id, briefing.GeneratedAt, briefing.Title, briefing.Period, briefing.Actions.Count
        );
        await _hub.Clients.All.SendAsync("NewBriefing", dto, ct);
        _logger.LogInformation("SignalR: NewBriefing sent for {Title}", briefing.Title);
    }

    public async Task SendAlertAsync(Alert alert, CancellationToken ct = default)
    {
        var dto = new AlertDto(
            alert.Id, alert.CreatedAt, alert.Severity, alert.Title, alert.Message, alert.Category, alert.IsAcknowledged
        );
        await _hub.Clients.All.SendAsync("NewAlert", dto, ct);
        _logger.LogInformation("SignalR: NewAlert sent for {Title}", alert.Title);
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
