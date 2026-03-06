using MarketingAssistant.Core.Enums;
using MarketingAssistant.Core.Interfaces;
using MarketingAssistant.Core.Models;
using MarketingAssistant.Infrastructure.Data;
using MarketingAssistant.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace MarketingAssistant.Infrastructure.Tests;

public class AlertServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly Mock<DataAggregator> _aggregator;
    private readonly Mock<INotificationService> _notifications;
    private readonly AlertService _alertService;

    public AlertServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _notifications = new Mock<INotificationService>();

        // DataAggregator has concrete dependencies, so we use a mock connector setup
        var woo = new Mock<IWooCommerceConnector>();
        var ga = new Mock<IGoogleAnalyticsConnector>();
        var ads = new Mock<IGoogleAdsConnector>();
        _aggregator = new Mock<DataAggregator>(woo.Object, ga.Object, ads.Object, _db);

        var logger = new Mock<ILogger<AlertService>>();
        _alertService = new AlertService(_aggregator.Object, _db, _notifications.Object, logger.Object);
    }

    [Fact]
    public async Task CheckAndCreateAlertsAsync_LowStockProducts_CreatesAlerts()
    {
        var data = new AggregatedData
        {
            LowStockProducts = [new Product { Name = "Honing", StockQuantity = 2 }],
            Ads = new AdsData { Roas = 5.0m },
            Analytics = new AnalyticsData { Visitors = 500, ConversionRate = 3.0m },
            RecentOrders = []
        };
        _aggregator.Setup(a => a.GetAggregatedDataAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(data);

        var alerts = await _alertService.CheckAndCreateAlertsAsync();

        Assert.Single(alerts);
        Assert.Equal("voorraad", alerts[0].Category);
        Assert.Equal(AlertSeverity.Warning, alerts[0].Severity);
        Assert.Contains("Honing", alerts[0].Title);
    }

    [Fact]
    public async Task CheckAndCreateAlertsAsync_ZeroStock_CreatesCriticalAlert()
    {
        var data = new AggregatedData
        {
            LowStockProducts = [new Product { Name = "Uitverkocht", StockQuantity = 0 }],
            Ads = new AdsData { Roas = 5.0m },
            Analytics = new AnalyticsData { Visitors = 500, ConversionRate = 3.0m },
            RecentOrders = []
        };
        _aggregator.Setup(a => a.GetAggregatedDataAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(data);

        var alerts = await _alertService.CheckAndCreateAlertsAsync();

        Assert.Single(alerts);
        Assert.Equal(AlertSeverity.Critical, alerts[0].Severity);
    }

    [Fact]
    public async Task CheckAndCreateAlertsAsync_LowRoas_CreatesAlert()
    {
        var data = new AggregatedData
        {
            LowStockProducts = [],
            Ads = new AdsData { Roas = 1.5m },
            Analytics = new AnalyticsData { Visitors = 500, ConversionRate = 3.0m },
            RecentOrders = []
        };
        _aggregator.Setup(a => a.GetAggregatedDataAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(data);

        var alerts = await _alertService.CheckAndCreateAlertsAsync();

        Assert.Single(alerts);
        Assert.Equal("advertenties", alerts[0].Category);
        Assert.Contains("ROAS", alerts[0].Title);
    }

    [Fact]
    public async Task CheckAndCreateAlertsAsync_NoIssues_ReturnsEmpty()
    {
        var data = new AggregatedData
        {
            LowStockProducts = [],
            Ads = new AdsData { Roas = 5.0m },
            Analytics = new AnalyticsData { Visitors = 500, ConversionRate = 3.0m },
            RecentOrders = []
        };
        _aggregator.Setup(a => a.GetAggregatedDataAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(data);

        var alerts = await _alertService.CheckAndCreateAlertsAsync();

        Assert.Empty(alerts);
    }

    [Fact]
    public async Task CheckAndCreateAlertsAsync_AlertsSavedToDatabase()
    {
        var data = new AggregatedData
        {
            LowStockProducts = [new Product { Name = "Product A", StockQuantity = 1 }],
            Ads = new AdsData { Roas = 5.0m },
            Analytics = new AnalyticsData { Visitors = 500, ConversionRate = 3.0m },
            RecentOrders = []
        };
        _aggregator.Setup(a => a.GetAggregatedDataAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(data);

        await _alertService.CheckAndCreateAlertsAsync();

        Assert.Equal(1, await _db.Alerts.CountAsync());
    }

    [Fact]
    public async Task CheckAndCreateAlertsAsync_SendsNotificationPerAlert()
    {
        var data = new AggregatedData
        {
            LowStockProducts =
            [
                new Product { Name = "Product A", StockQuantity = 1 },
                new Product { Name = "Product B", StockQuantity = 3 }
            ],
            Ads = new AdsData { Roas = 5.0m },
            Analytics = new AnalyticsData { Visitors = 500, ConversionRate = 3.0m },
            RecentOrders = []
        };
        _aggregator.Setup(a => a.GetAggregatedDataAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(data);

        await _alertService.CheckAndCreateAlertsAsync();

        _notifications.Verify(n => n.SendAlertAsync(
            It.IsAny<Alert>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
