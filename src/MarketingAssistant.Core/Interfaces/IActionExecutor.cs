using MarketingAssistant.Core.Models;

namespace MarketingAssistant.Core.Interfaces;

public interface IActionExecutor
{
    Task<ActionItem> ApproveAsync(int actionId, string approvedBy, CancellationToken ct = default);
    Task<ActionItem> RejectAsync(int actionId, string rejectedBy, CancellationToken ct = default);
    Task ExecuteAsync(ActionItem action, CancellationToken ct = default);
}
