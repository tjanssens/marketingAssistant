using MarketingAssistant.Core.Interfaces;
using MarketingAssistant.Core.Models;
using MarketingAssistant.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace MarketingAssistant.Infrastructure.Services;

public class BriefingService
{
    private readonly DataAggregator _aggregator;
    private readonly IAiBrainService _ai;
    private readonly AppDbContext _db;
    private readonly ILogger<BriefingService> _logger;

    public BriefingService(DataAggregator aggregator, IAiBrainService ai, AppDbContext db, ILogger<BriefingService> logger)
    {
        _aggregator = aggregator;
        _ai = ai;
        _db = db;
        _logger = logger;
    }

    public async Task<Briefing> GenerateAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Generating daily briefing...");

        var data = await _aggregator.GetAggregatedDataAsync(ct: ct);
        var briefing = await _ai.GenerateBriefingAsync(data, ct);

        _db.Briefings.Add(briefing);
        await _db.SaveChangesAsync(ct);

        await _aggregator.CreateSnapshotAsync(data, ct);

        _logger.LogInformation("Briefing generated: {Title} with {ActionCount} actions", briefing.Title, briefing.Actions.Count);
        return briefing;
    }
}
