using MarketingAssistant.Core.Interfaces;
using MarketingAssistant.Core.Models;
using MarketingAssistant.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarketingAssistant.Infrastructure.Services;

public class DataAggregator
{
    private readonly IWooCommerceConnector _wooCommerce;
    private readonly IGoogleAnalyticsConnector _analytics;
    private readonly IGoogleAdsConnector _ads;
    private readonly AppDbContext _db;

    public DataAggregator(
        IWooCommerceConnector wooCommerce,
        IGoogleAnalyticsConnector analytics,
        IGoogleAdsConnector ads,
        AppDbContext db)
    {
        _wooCommerce = wooCommerce;
        _analytics = analytics;
        _ads = ads;
        _db = db;
    }

    public async Task<AggregatedData> GetAggregatedDataAsync(int days = 7, CancellationToken ct = default)
    {
        var ordersTask = _wooCommerce.GetRecentOrdersAsync(days, ct);
        var productsTask = _wooCommerce.GetProductsAsync(ct);
        var lowStockTask = _wooCommerce.GetLowStockProductsAsync(5, ct);
        var analyticsTask = _analytics.GetAnalyticsDataAsync(days, ct);
        var adsTask = _ads.GetAdsDataAsync(days, ct);
        var previousKpiTask = _db.KpiSnapshots
            .OrderByDescending(k => k.CapturedAt)
            .FirstOrDefaultAsync(ct);

        await Task.WhenAll(ordersTask, productsTask, lowStockTask, analyticsTask, adsTask, previousKpiTask);

        return new AggregatedData
        {
            RecentOrders = await ordersTask,
            Products = await productsTask,
            LowStockProducts = await lowStockTask,
            Analytics = await analyticsTask,
            Ads = await adsTask,
            PreviousKpi = await previousKpiTask
        };
    }

    public async Task<KpiSnapshot> CreateSnapshotAsync(AggregatedData data, CancellationToken ct = default)
    {
        var snapshot = new KpiSnapshot
        {
            CapturedAt = DateTime.UtcNow,
            OrderCount = data.RecentOrders.Count,
            Revenue = data.RecentOrders.Sum(o => o.Total),
            ConversionRate = data.Analytics.ConversionRate,
            Visitors = data.Analytics.Visitors,
            LowStockCount = data.LowStockProducts.Count,
            AdSpend = data.Ads.TotalSpend,
            Roas = data.Ads.Roas
        };

        _db.KpiSnapshots.Add(snapshot);
        await _db.SaveChangesAsync(ct);
        return snapshot;
    }
}
