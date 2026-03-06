using MarketingAssistant.Core.DTOs;
using MarketingAssistant.Core.Enums;
using Microsoft.AspNetCore.Mvc;

namespace MarketingAssistant.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    [HttpGet]
    public ActionResult<DashboardDto> Get()
    {
        // TODO: Replace with real service in Fase 2
        var kpis = new KpiDto(
            OrderCount: 0,
            Revenue: 0m,
            ConversionRate: 0m,
            Visitors: 0,
            LowStockCount: 0,
            AdSpend: 0m,
            Roas: 0m
        );

        var dashboard = new DashboardDto(
            Kpis: kpis,
            RecentAlerts: [],
            PendingActionCount: 0,
            LatestBriefing: null
        );

        return Ok(dashboard);
    }
}
