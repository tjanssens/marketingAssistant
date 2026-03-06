using MarketingAssistant.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MarketingAssistant.Scheduling;

public class HourlyAlertCheckJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<HourlyAlertCheckJob> _logger;
    private readonly TimeSpan _interval;

    public HourlyAlertCheckJob(IServiceScopeFactory scopeFactory, ILogger<HourlyAlertCheckJob> logger, IConfiguration config)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        var minutes = int.Parse(config["Scheduling:AlertIntervalMinutes"] ?? "60");
        _interval = TimeSpan.FromMinutes(minutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Alert check job started, interval: {Interval}", _interval);

        // Wait a bit before first check to let the app fully start
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var alertService = scope.ServiceProvider.GetRequiredService<AlertService>();
                await alertService.CheckAndCreateAlertsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Failed to run alert check");
            }

            try
            {
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
