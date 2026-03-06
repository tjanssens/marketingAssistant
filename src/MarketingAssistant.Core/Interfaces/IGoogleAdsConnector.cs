namespace MarketingAssistant.Core.Interfaces;

public class AdsData
{
    public decimal TotalSpend { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal Roas { get; set; }
    public int Clicks { get; set; }
    public int Impressions { get; set; }
    public decimal Ctr { get; set; }
    public List<CampaignData> Campaigns { get; set; } = [];
}

public class CampaignData
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public decimal Spend { get; set; }
    public decimal Roas { get; set; }
    public int Clicks { get; set; }
    public int Conversions { get; set; }
}

public interface IGoogleAdsConnector
{
    Task<AdsData> GetAdsDataAsync(int days = 7, CancellationToken ct = default);
    Task<IReadOnlyList<CampaignData>> GetCampaignsAsync(CancellationToken ct = default);
}
