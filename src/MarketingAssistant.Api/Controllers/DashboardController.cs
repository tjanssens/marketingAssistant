using MarketingAssistant.Core.DTOs;
using MarketingAssistant.Core.Enums;
using MarketingAssistant.Infrastructure.Data;
using MarketingAssistant.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketingAssistant.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly DataAggregator _aggregator;
    private readonly AppDbContext _db;

    public DashboardController(DataAggregator aggregator, AppDbContext db)
    {
        _aggregator = aggregator;
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get(CancellationToken ct)
    {
        var data = await _aggregator.GetAggregatedDataAsync(ct: ct);

        var kpis = new KpiDto(
            OrderCount: data.RecentOrders.Count,
            Revenue: data.RecentOrders.Sum(o => o.Total),
            ConversionRate: data.Analytics.ConversionRate,
            Visitors: data.Analytics.Visitors,
            LowStockCount: data.LowStockProducts.Count,
            AdSpend: data.Ads.TotalSpend,
            Roas: data.Ads.Roas
        );

        var recentAlerts = await _db.Alerts
            .OrderByDescending(a => a.CreatedAt)
            .Take(10)
            .Select(a => new AlertDto(
                a.Id, a.CreatedAt, a.Severity, a.Title, a.Message, a.Category, a.IsAcknowledged
            ))
            .ToListAsync(ct);

        var pendingCount = await _db.ActionItems
            .CountAsync(a => a.Status == ActionStatus.Pending, ct);

        var latestBriefing = await _db.Briefings
            .OrderByDescending(b => b.GeneratedAt)
            .Select(b => new BriefingSummaryDto(
                b.Id, b.GeneratedAt, b.Title, b.Period, b.Actions.Count
            ))
            .FirstOrDefaultAsync(ct);

        var dashboard = new DashboardDto(
            Kpis: kpis,
            RecentAlerts: recentAlerts,
            PendingActionCount: pendingCount,
            LatestBriefing: latestBriefing
        );

        return Ok(dashboard);
    }
}
