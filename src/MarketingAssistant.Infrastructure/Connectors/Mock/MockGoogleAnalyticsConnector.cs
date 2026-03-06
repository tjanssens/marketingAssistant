using MarketingAssistant.Core.Interfaces;

namespace MarketingAssistant.Infrastructure.Connectors.Mock;

public class MockGoogleAnalyticsConnector : IGoogleAnalyticsConnector
{
    public Task<AnalyticsData> GetAnalyticsDataAsync(int days = 7, CancellationToken ct = default)
    {
        var data = new AnalyticsData
        {
            Visitors = 2847,
            Sessions = 3412,
            BounceRate = 42.3m,
            ConversionRate = 3.2m,
            TopPages = new Dictionary<string, int>
            {
                { "/", 1245 },
                { "/producten/biologische-honing-500g", 487 },
                { "/producten/manuka-honing-mgo-400", 356 },
                { "/producten/lippenbalsem-bijenwas", 298 },
                { "/categorie/honing", 267 },
                { "/over-ons", 189 },
                { "/producten/cadeaupakket-de-imker", 156 },
                { "/contact", 134 },
                { "/blog/gezondheidsvoordelen-honing", 112 },
                { "/winkelwagen", 98 }
            },
            TrafficSources = new Dictionary<string, int>
            {
                { "Organic Search", 1245 },
                { "Direct", 678 },
                { "Social", 423 },
                { "Paid Search", 312 },
                { "Referral", 189 }
            }
        };

        return Task.FromResult(data);
    }
}
