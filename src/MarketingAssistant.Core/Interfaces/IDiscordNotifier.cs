using MarketingAssistant.Core.Models;

namespace MarketingAssistant.Core.Interfaces;

public interface IDiscordNotifier
{
    Task SendBriefingAsync(Briefing briefing, CancellationToken ct = default);
    Task SendAlertAsync(Alert alert, CancellationToken ct = default);
    Task SendActionWithButtonsAsync(ActionItem action, CancellationToken ct = default);
}
