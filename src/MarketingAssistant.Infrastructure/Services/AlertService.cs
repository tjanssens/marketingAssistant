using MarketingAssistant.Core.Enums;
using MarketingAssistant.Core.Interfaces;
using MarketingAssistant.Core.Models;
using MarketingAssistant.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace MarketingAssistant.Infrastructure.Services;

public class AlertService
{
    private readonly DataAggregator _aggregator;
    private readonly AppDbContext _db;
    private readonly INotificationService _notifications;
    private readonly ILogger<AlertService> _logger;

    public AlertService(DataAggregator aggregator, AppDbContext db,
        INotificationService notifications, ILogger<AlertService> logger)
    {
        _aggregator = aggregator;
        _db = db;
        _notifications = notifications;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Alert>> CheckAndCreateAlertsAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Running alert checks...");

        var data = await _aggregator.GetAggregatedDataAsync(ct: ct);
        var alerts = new List<Alert>();

        // Low stock check
        foreach (var product in data.LowStockProducts)
        {
            var alert = new Alert
            {
                CreatedAt = DateTime.UtcNow,
                Severity = product.StockQuantity == 0 ? AlertSeverity.Critical : AlertSeverity.Warning,
                Title = $"Lage voorraad: {product.Name}",
                Message = $"{product.Name} heeft nog {product.StockQuantity} stuks op voorraad.",
                Category = "voorraad"
            };
            alerts.Add(alert);
        }

        // ROAS check — if below 2.0 it's not profitable
        if (data.Ads.Roas > 0 && data.Ads.Roas < 2.0m)
        {
            alerts.Add(new Alert
            {
                CreatedAt = DateTime.UtcNow,
                Severity = data.Ads.Roas < 1.0m ? AlertSeverity.Critical : AlertSeverity.Warning,
                Title = "Lage ROAS",
                Message = $"ROAS is {data.Ads.Roas:F2}x — advertenties zijn {(data.Ads.Roas < 1.0m ? "verliesgevend" : "nauwelijks winstgevend")}.",
                Category = "advertenties"
            });
        }

        // Revenue drop check — compare with previous KPI
        if (data.PreviousKpi is not null && data.PreviousKpi.Revenue > 0)
        {
            var currentRevenue = data.RecentOrders.Sum(o => o.Total);
            var revenueChange = (currentRevenue - data.PreviousKpi.Revenue) / data.PreviousKpi.Revenue;

            if (revenueChange < -0.2m) // 20% drop
            {
                alerts.Add(new Alert
                {
                    CreatedAt = DateTime.UtcNow,
                    Severity = revenueChange < -0.5m ? AlertSeverity.Critical : AlertSeverity.Warning,
                    Title = "Omzetdaling",
                    Message = $"Omzet is {Math.Abs(revenueChange):P0} gedaald ten opzichte van vorige periode (€{data.PreviousKpi.Revenue:F2} → €{currentRevenue:F2}).",
                    Category = "omzet"
                });
            }
        }

        // Conversion rate check
        if (data.Analytics.Visitors > 100 && data.Analytics.ConversionRate < 1.0m)
        {
            alerts.Add(new Alert
            {
                CreatedAt = DateTime.UtcNow,
                Severity = data.Analytics.ConversionRate < 0.5m ? AlertSeverity.Critical : AlertSeverity.Warning,
                Title = "Lage conversieratio",
                Message = $"Conversieratio is {data.Analytics.ConversionRate:F1}% bij {data.Analytics.Visitors} bezoekers.",
                Category = "conversie"
            });
        }

        // Ad spend without conversions
        foreach (var campaign in data.Ads.Campaigns.Where(c => c.Status == "Active"))
        {
            if (campaign.Spend > 20 && campaign.Conversions == 0)
            {
                alerts.Add(new Alert
                {
                    CreatedAt = DateTime.UtcNow,
                    Severity = campaign.Spend > 50 ? AlertSeverity.Critical : AlertSeverity.Warning,
                    Title = $"Campagne zonder conversies: {campaign.Name}",
                    Message = $"Campagne '{campaign.Name}' heeft €{campaign.Spend:F2} uitgegeven zonder conversies.",
                    Category = "advertenties"
                });
            }
        }

        if (alerts.Count > 0)
        {
            _db.Alerts.AddRange(alerts);
            await _db.SaveChangesAsync(ct);

            foreach (var alert in alerts)
            {
                await _notifications.SendAlertAsync(alert, ct);
            }

            _logger.LogInformation("Created {Count} alerts", alerts.Count);
        }
        else
        {
            _logger.LogInformation("No alerts triggered");
        }

        return alerts;
    }
}
