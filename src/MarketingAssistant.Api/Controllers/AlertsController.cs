using MarketingAssistant.Core.DTOs;
using MarketingAssistant.Infrastructure.Data;
using MarketingAssistant.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketingAssistant.Api.Controllers;

[ApiController]
[Route("api/alerts")]
public class AlertsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AlertService _alertService;

    public AlertsController(AppDbContext db, AlertService alertService)
    {
        _db = db;
        _alertService = alertService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AlertDto>>> GetAll(CancellationToken ct)
    {
        var alerts = await _db.Alerts
            .OrderByDescending(a => a.CreatedAt)
            .Take(50)
            .Select(a => new AlertDto(
                a.Id, a.CreatedAt, a.Severity, a.Title, a.Message, a.Category, a.IsAcknowledged
            ))
            .ToListAsync(ct);

        return Ok(alerts);
    }

    [HttpPost("check")]
    public async Task<ActionResult<List<AlertDto>>> RunCheck(CancellationToken ct)
    {
        var alerts = await _alertService.CheckAndCreateAlertsAsync(ct);

        return Ok(alerts.Select(a => new AlertDto(
            a.Id, a.CreatedAt, a.Severity, a.Title, a.Message, a.Category, a.IsAcknowledged
        )).ToList());
    }
}
