using MarketingAssistant.Core.Interfaces;

namespace MarketingAssistant.Infrastructure.Connectors.Mock;

public class MockGoogleAdsConnector : IGoogleAdsConnector
{
    private static readonly List<CampaignData> Campaigns =
    [
        new()
        {
            Id = "camp-001", Name = "Honing - Brand", Status = "ENABLED",
            Budget = 25.00m, Spend = 22.45m, Roas = 4.2m, Clicks = 187, Conversions = 12
        },
        new()
        {
            Id = "camp-002", Name = "Manuka Honing - Shopping", Status = "ENABLED",
            Budget = 40.00m, Spend = 38.70m, Roas = 5.8m, Clicks = 245, Conversions = 18
        },
        new()
        {
            Id = "camp-003", Name = "Cadeaupakketten - Seizoen", Status = "ENABLED",
            Budget = 15.00m, Spend = 12.30m, Roas = 3.1m, Clicks = 98, Conversions = 5
        },
        new()
        {
            Id = "camp-004", Name = "Bijenwas Producten", Status = "PAUSED",
            Budget = 10.00m, Spend = 0m, Roas = 0m, Clicks = 0, Conversions = 0
        },
        new()
        {
            Id = "camp-005", Name = "Retargeting - Winkelwagen", Status = "ENABLED",
            Budget = 20.00m, Spend = 17.85m, Roas = 7.3m, Clicks = 156, Conversions = 14
        }
    ];

    public Task<AdsData> GetAdsDataAsync(int days = 7, CancellationToken ct = default)
    {
        var activeCampaigns = Campaigns.Where(c => c.Status == "ENABLED").ToList();
        var totalSpend = activeCampaigns.Sum(c => c.Spend);
        var totalClicks = activeCampaigns.Sum(c => c.Clicks);
        var totalImpressions = totalClicks * 18; // ~5.5% CTR
        var totalRevenue = activeCampaigns.Sum(c => c.Spend * c.Roas);

        var data = new AdsData
        {
            TotalSpend = totalSpend,
            TotalRevenue = totalRevenue,
            Roas = totalRevenue / totalSpend,
            Clicks = totalClicks,
            Impressions = totalImpressions,
            Ctr = (decimal)totalClicks / totalImpressions * 100,
            Campaigns = Campaigns
        };

        return Task.FromResult(data);
    }

    public Task<IReadOnlyList<CampaignData>> GetCampaignsAsync(CancellationToken ct = default)
    {
        return Task.FromResult<IReadOnlyList<CampaignData>>(Campaigns);
    }
}
