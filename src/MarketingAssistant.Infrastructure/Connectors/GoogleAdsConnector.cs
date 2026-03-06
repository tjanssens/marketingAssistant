using Google.Ads.GoogleAds;
using Google.Ads.GoogleAds.Config;
using Google.Ads.GoogleAds.Lib;
using Google.Ads.GoogleAds.V23.Services;
using MarketingAssistant.Core.Interfaces;
using MarketingAssistant.Infrastructure.Connectors.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using GaServices = Google.Ads.GoogleAds.Services;

namespace MarketingAssistant.Infrastructure.Connectors;

public class GoogleAdsConnector : IGoogleAdsConnector
{
    private readonly GoogleAdsOptions _options;
    private readonly ILogger<GoogleAdsConnector> _logger;

    public GoogleAdsConnector(IOptions<GoogleAdsOptions> options, ILogger<GoogleAdsConnector> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AdsData> GetAdsDataAsync(int days = 7, CancellationToken ct = default)
    {
        var client = CreateClient();
        var service = client.GetService(GaServices.V23.GoogleAdsService);

        var query = $@"
            SELECT
                campaign.id,
                campaign.name,
                campaign.status,
                campaign_budget.amount_micros,
                metrics.cost_micros,
                metrics.conversions_value,
                metrics.clicks,
                metrics.impressions,
                metrics.conversions
            FROM campaign
            WHERE segments.date DURING LAST_{days}_DAYS
              AND campaign.status != 'REMOVED'";

        var request = new SearchGoogleAdsRequest
        {
            CustomerId = _options.CustomerAccountId.Replace("-", ""),
            Query = query
        };

        var result = new AdsData();
        var campaigns = new List<CampaignData>();

        var response = service.Search(request);
        foreach (var row in response)
        {
            var costMicros = row.Metrics.CostMicros;
            var revenueMicros = (long)(row.Metrics.ConversionsValue * 1_000_000);
            var spend = costMicros / 1_000_000m;
            var revenue = revenueMicros / 1_000_000m;

            result.TotalSpend += spend;
            result.TotalRevenue += revenue;
            result.Clicks += (int)row.Metrics.Clicks;
            result.Impressions += (int)row.Metrics.Impressions;

            campaigns.Add(new CampaignData
            {
                Id = row.Campaign.Id.ToString(),
                Name = row.Campaign.Name,
                Status = row.Campaign.Status.ToString(),
                Budget = row.CampaignBudget?.AmountMicros > 0
                    ? row.CampaignBudget.AmountMicros / 1_000_000m
                    : 0,
                Spend = spend,
                Roas = spend > 0 ? Math.Round(revenue / spend, 2) : 0,
                Clicks = (int)row.Metrics.Clicks,
                Conversions = (int)row.Metrics.Conversions
            });
        }

        result.Campaigns = campaigns;
        result.Roas = result.TotalSpend > 0
            ? Math.Round(result.TotalRevenue / result.TotalSpend, 2)
            : 0;
        result.Ctr = result.Impressions > 0
            ? Math.Round((decimal)result.Clicks / result.Impressions * 100, 2)
            : 0;

        _logger.LogInformation("Google Ads: €{Spend:F2} spend, {Roas:F2}x ROAS, {Campaigns} campaigns",
            result.TotalSpend, result.Roas, campaigns.Count);

        return await Task.FromResult(result);
    }

    public async Task<IReadOnlyList<CampaignData>> GetCampaignsAsync(CancellationToken ct = default)
    {
        var data = await GetAdsDataAsync(ct: ct);
        return data.Campaigns;
    }

    private GoogleAdsClient CreateClient()
    {
        var config = new GoogleAdsConfig
        {
            DeveloperToken = _options.DeveloperToken,
            OAuth2ClientId = _options.ClientId,
            OAuth2ClientSecret = _options.ClientSecret,
            OAuth2RefreshToken = _options.RefreshToken,
            LoginCustomerId = _options.CustomerAccountId.Replace("-", "")
        };

        return new GoogleAdsClient(config);
    }
}
