using MarketingAssistant.Core.Enums;
using MarketingAssistant.Core.Interfaces;
using MarketingAssistant.Core.Models;
using MarketingAssistant.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace MarketingAssistant.Infrastructure.Services;

public class ActionExecutor : IActionExecutor
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notifications;
    private readonly ILogger<ActionExecutor> _logger;

    public ActionExecutor(AppDbContext db, INotificationService notifications, ILogger<ActionExecutor> logger)
    {
        _db = db;
        _notifications = notifications;
        _logger = logger;
    }

    public async Task<ActionItem> ApproveAsync(int actionId, string approvedBy, CancellationToken ct = default)
    {
        var action = await _db.ActionItems.FindAsync([actionId], ct)
            ?? throw new KeyNotFoundException($"ActionItem {actionId} not found");

        if (action.Status != ActionStatus.Pending)
            throw new InvalidOperationException($"ActionItem {actionId} is not Pending (current: {action.Status})");

        action.Status = ActionStatus.Approved;
        action.ResolvedAt = DateTime.UtcNow;
        action.ResolvedBy = approvedBy;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Action {Id} approved by {ApprovedBy}: {Description}", actionId, approvedBy, action.Description);
        await _notifications.SendActionUpdateAsync(action, ct);

        return action;
    }

    public async Task<ActionItem> RejectAsync(int actionId, string rejectedBy, CancellationToken ct = default)
    {
        var action = await _db.ActionItems.FindAsync([actionId], ct)
            ?? throw new KeyNotFoundException($"ActionItem {actionId} not found");

        if (action.Status != ActionStatus.Pending)
            throw new InvalidOperationException($"ActionItem {actionId} is not Pending (current: {action.Status})");

        action.Status = ActionStatus.Rejected;
        action.ResolvedAt = DateTime.UtcNow;
        action.ResolvedBy = rejectedBy;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Action {Id} rejected by {RejectedBy}: {Description}", actionId, rejectedBy, action.Description);
        await _notifications.SendActionUpdateAsync(action, ct);

        return action;
    }

    public async Task ExecuteAsync(ActionItem action, CancellationToken ct = default)
    {
        // MVP: just mark as executed, no real execution
        action.Status = ActionStatus.Executed;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Action {Id} executed: {Description}", action.Id, action.Description);
        await _notifications.SendActionUpdateAsync(action, ct);
    }
}
