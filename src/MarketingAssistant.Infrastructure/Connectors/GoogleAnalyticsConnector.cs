using Google.Analytics.Data.V1Beta;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using MarketingAssistant.Core.Interfaces;
using MarketingAssistant.Infrastructure.Connectors.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MarketingAssistant.Infrastructure.Connectors;

public class GoogleAnalyticsConnector : IGoogleAnalyticsConnector
{
    private readonly GoogleAnalyticsOptions _options;
    private readonly ILogger<GoogleAnalyticsConnector> _logger;

    public GoogleAnalyticsConnector(IOptions<GoogleAnalyticsOptions> options, ILogger<GoogleAnalyticsConnector> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AnalyticsData> GetAnalyticsDataAsync(int days = 7, CancellationToken ct = default)
    {
        var client = await CreateClientAsync();

        var request = new RunReportRequest
        {
            Property = $"properties/{_options.PropertyId}",
            DateRanges =
            {
                new DateRange { StartDate = $"{days}daysAgo", EndDate = "today" }
            },
            Dimensions =
            {
                new Dimension { Name = "sessionDefaultChannelGroup" }
            },
            Metrics =
            {
                new Metric { Name = "totalUsers" },
                new Metric { Name = "sessions" },
                new Metric { Name = "bounceRate" },
                new Metric { Name = "conversions" },
                new Metric { Name = "screenPageViews" }
            }
        };

        var response = await client.RunReportAsync(request, ct);

        var result = new AnalyticsData();
        var trafficSources = new Dictionary<string, int>();

        foreach (var row in response.Rows)
        {
            var channel = row.DimensionValues[0].Value;
            var users = int.TryParse(row.MetricValues[0].Value, out var u) ? u : 0;
            var sessions = int.TryParse(row.MetricValues[1].Value, out var s) ? s : 0;

            result.Visitors += users;
            result.Sessions += sessions;
            trafficSources[channel] = users;
        }

        // Get totals from response metadata
        if (response.Totals.Count > 0)
        {
            var totals = response.Totals[0];
            result.Visitors = int.TryParse(totals.MetricValues[0].Value, out var tv) ? tv : result.Visitors;
            result.Sessions = int.TryParse(totals.MetricValues[1].Value, out var ts) ? ts : result.Sessions;
            result.BounceRate = decimal.TryParse(totals.MetricValues[2].Value, out var br) ? br * 100 : 0;

            var conversions = int.TryParse(totals.MetricValues[3].Value, out var c) ? c : 0;
            result.ConversionRate = result.Sessions > 0
                ? Math.Round((decimal)conversions / result.Sessions * 100, 2)
                : 0;
        }

        result.TrafficSources = trafficSources;

        // Get top pages
        await LoadTopPages(client, days, result, ct);

        _logger.LogInformation("GA4: {Visitors} visitors, {Sessions} sessions, {ConversionRate}% conversion",
            result.Visitors, result.Sessions, result.ConversionRate);

        return result;
    }

    private async Task LoadTopPages(BetaAnalyticsDataClient client, int days, AnalyticsData result, CancellationToken ct)
    {
        var pageRequest = new RunReportRequest
        {
            Property = $"properties/{_options.PropertyId}",
            DateRanges =
            {
                new DateRange { StartDate = $"{days}daysAgo", EndDate = "today" }
            },
            Dimensions = { new Dimension { Name = "pagePath" } },
            Metrics = { new Metric { Name = "screenPageViews" } },
            Limit = 10,
            OrderBys =
            {
                new OrderBy
                {
                    Metric = new OrderBy.Types.MetricOrderBy { MetricName = "screenPageViews" },
                    Desc = true
                }
            }
        };

        var pageResponse = await client.RunReportAsync(pageRequest, ct);
        result.TopPages = pageResponse.Rows
            .ToDictionary(
                r => r.DimensionValues[0].Value,
                r => int.TryParse(r.MetricValues[0].Value, out var v) ? v : 0
            );
    }

    private async Task<BetaAnalyticsDataClient> CreateClientAsync()
    {
        if (!string.IsNullOrEmpty(_options.CredentialsJson))
        {
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(_options.CredentialsJson));
            var serviceCredential = ServiceAccountCredential.FromServiceAccountData(stream);
            var builder = new BetaAnalyticsDataClientBuilder
            {
                GoogleCredential = serviceCredential.ToGoogleCredential()
            };
            return await builder.BuildAsync();
        }

        // Fall back to Application Default Credentials
        return await new BetaAnalyticsDataClientBuilder().BuildAsync();
    }
}
