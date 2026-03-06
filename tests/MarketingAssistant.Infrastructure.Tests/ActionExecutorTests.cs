using MarketingAssistant.Core.Enums;
using MarketingAssistant.Core.Interfaces;
using MarketingAssistant.Core.Models;
using MarketingAssistant.Infrastructure.Data;
using MarketingAssistant.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace MarketingAssistant.Infrastructure.Tests;

public class ActionExecutorTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly Mock<INotificationService> _notifications;
    private readonly ActionExecutor _executor;

    public ActionExecutorTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _notifications = new Mock<INotificationService>();
        var logger = new Mock<ILogger<ActionExecutor>>();
        _executor = new ActionExecutor(_db, _notifications.Object, logger.Object);
    }

    [Fact]
    public async Task ApproveAsync_PendingAction_SetsApprovedStatus()
    {
        var action = new ActionItem
        {
            Description = "Test action",
            Type = ActionType.BudgetShift,
            Status = ActionStatus.Pending,
            SuggestedAt = DateTime.UtcNow,
            AiReasoning = "Test reasoning"
        };
        _db.ActionItems.Add(action);
        await _db.SaveChangesAsync();

        var result = await _executor.ApproveAsync(action.Id, "test-user");

        Assert.Equal(ActionStatus.Approved, result.Status);
        Assert.Equal("test-user", result.ResolvedBy);
        Assert.NotNull(result.ResolvedAt);
    }

    [Fact]
    public async Task ApproveAsync_PendingAction_SendsNotification()
    {
        var action = new ActionItem
        {
            Description = "Test action",
            Type = ActionType.StockAlert,
            Status = ActionStatus.Pending,
            SuggestedAt = DateTime.UtcNow,
            AiReasoning = "Low stock"
        };
        _db.ActionItems.Add(action);
        await _db.SaveChangesAsync();

        await _executor.ApproveAsync(action.Id, "dashboard");

        _notifications.Verify(n => n.SendActionUpdateAsync(
            It.Is<ActionItem>(a => a.Status == ActionStatus.Approved),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApproveAsync_NonPendingAction_ThrowsInvalidOperation()
    {
        var action = new ActionItem
        {
            Description = "Already approved",
            Type = ActionType.PriceAdjust,
            Status = ActionStatus.Approved,
            SuggestedAt = DateTime.UtcNow,
            AiReasoning = "Price too high"
        };
        _db.ActionItems.Add(action);
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _executor.ApproveAsync(action.Id, "test-user"));
    }

    [Fact]
    public async Task ApproveAsync_NonExistentAction_ThrowsKeyNotFound()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _executor.ApproveAsync(999, "test-user"));
    }

    [Fact]
    public async Task RejectAsync_PendingAction_SetsRejectedStatus()
    {
        var action = new ActionItem
        {
            Description = "Bad suggestion",
            Type = ActionType.PauseCampaign,
            Status = ActionStatus.Pending,
            SuggestedAt = DateTime.UtcNow,
            AiReasoning = "Campaign underperforming"
        };
        _db.ActionItems.Add(action);
        await _db.SaveChangesAsync();

        var result = await _executor.RejectAsync(action.Id, "test-user");

        Assert.Equal(ActionStatus.Rejected, result.Status);
        Assert.Equal("test-user", result.ResolvedBy);
    }

    [Fact]
    public async Task ExecuteAsync_SetsExecutedStatus()
    {
        var action = new ActionItem
        {
            Description = "Execute this",
            Type = ActionType.ContentPost,
            Status = ActionStatus.Approved,
            SuggestedAt = DateTime.UtcNow,
            AiReasoning = "Good to go"
        };
        _db.ActionItems.Add(action);
        await _db.SaveChangesAsync();

        await _executor.ExecuteAsync(action);

        Assert.Equal(ActionStatus.Executed, action.Status);
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
