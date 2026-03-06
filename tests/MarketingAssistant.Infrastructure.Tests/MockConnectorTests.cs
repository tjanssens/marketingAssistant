using MarketingAssistant.Infrastructure.Connectors.Mock;

namespace MarketingAssistant.Infrastructure.Tests;

public class MockConnectorTests
{
    [Fact]
    public async Task MockWooCommerce_GetRecentOrders_ReturnsOrders()
    {
        var connector = new MockWooCommerceConnector();
        var orders = await connector.GetRecentOrdersAsync();

        Assert.NotEmpty(orders);
        Assert.All(orders, o => Assert.True(o.Total > 0));
        Assert.All(orders, o => Assert.NotEmpty(o.Items));
    }

    [Fact]
    public async Task MockWooCommerce_GetLowStockProducts_ReturnsLowStockOnly()
    {
        var connector = new MockWooCommerceConnector();
        var products = await connector.GetLowStockProductsAsync(5);

        Assert.NotEmpty(products);
        Assert.All(products, p => Assert.True(p.StockQuantity <= 5));
    }

    [Fact]
    public async Task MockGoogleAnalytics_GetAnalyticsData_ReturnsValidData()
    {
        var connector = new MockGoogleAnalyticsConnector();
        var data = await connector.GetAnalyticsDataAsync();

        Assert.True(data.Visitors > 0);
        Assert.True(data.Sessions > 0);
        Assert.True(data.ConversionRate > 0);
        Assert.NotEmpty(data.TopPages);
        Assert.NotEmpty(data.TrafficSources);
    }

    [Fact]
    public async Task MockGoogleAds_GetAdsData_ReturnsValidData()
    {
        var connector = new MockGoogleAdsConnector();
        var data = await connector.GetAdsDataAsync();

        Assert.True(data.TotalSpend > 0);
        Assert.True(data.Roas > 0);
        Assert.NotEmpty(data.Campaigns);
        Assert.All(data.Campaigns, c => Assert.NotEmpty(c.Name));
    }

    [Fact]
    public async Task MockGoogleAds_GetCampaigns_ReturnsData()
    {
        var connector = new MockGoogleAdsConnector();
        var campaigns = await connector.GetCampaignsAsync();

        Assert.NotEmpty(campaigns);
    }

    [Fact]
    public async Task MockGoogleAds_Roas_NoDivisionByZero()
    {
        var connector = new MockGoogleAdsConnector();
        var data = await connector.GetAdsDataAsync();

        // Should not throw or return NaN/Infinity
        Assert.False(decimal.IsNegative(data.Roas));
        Assert.All(data.Campaigns, c => Assert.False(decimal.IsNegative(c.Roas)));
    }
}
