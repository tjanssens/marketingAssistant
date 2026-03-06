using MarketingAssistant.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace MarketingAssistant.Scheduling;

public class DailyBriefingJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DailyBriefingJob> _logger;
    private readonly TimeOnly _briefingTime;

    public DailyBriefingJob(IServiceScopeFactory scopeFactory, ILogger<DailyBriefingJob> logger, IConfiguration config)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _briefingTime = TimeOnly.Parse(config["Scheduling:BriefingTime"] ?? "07:00");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Daily briefing job started, scheduled at {Time} UTC", _briefingTime);

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextRun = DateOnly.FromDateTime(now).ToDateTime(_briefingTime, DateTimeKind.Utc);

            if (nextRun <= now)
                nextRun = nextRun.AddDays(1);

            var delay = nextRun - now;
            _logger.LogInformation("Next briefing in {Hours:F1} hours at {NextRun:u}", delay.TotalHours, nextRun);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var briefingService = scope.ServiceProvider.GetRequiredService<BriefingService>();
                await briefingService.GenerateAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Failed to generate daily briefing");
            }
        }
    }
}
