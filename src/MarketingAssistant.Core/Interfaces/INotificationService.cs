using MarketingAssistant.Core.Models;

namespace MarketingAssistant.Core.Interfaces;

public interface INotificationService
{
    Task SendBriefingAsync(Briefing briefing, CancellationToken ct = default);
    Task SendAlertAsync(Alert alert, CancellationToken ct = default);
    Task SendActionUpdateAsync(ActionItem action, CancellationToken ct = default);
}
