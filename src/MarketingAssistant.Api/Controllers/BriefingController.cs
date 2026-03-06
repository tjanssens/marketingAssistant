using MarketingAssistant.Core.DTOs;
using MarketingAssistant.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketingAssistant.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BriefingController : ControllerBase
{
    private readonly AppDbContext _db;

    public BriefingController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<BriefingSummaryDto>>> GetAll(CancellationToken ct)
    {
        var briefings = await _db.Briefings
            .OrderByDescending(b => b.GeneratedAt)
            .Select(b => new BriefingSummaryDto(
                b.Id,
                b.GeneratedAt,
                b.Title,
                b.Period,
                b.Actions.Count
            ))
            .Take(50)
            .ToListAsync(ct);

        return Ok(briefings);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BriefingDto>> GetById(int id, CancellationToken ct)
    {
        var briefing = await _db.Briefings
            .Include(b => b.Actions)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (briefing is null)
            return NotFound();

        var dto = new BriefingDto(
            briefing.Id,
            briefing.GeneratedAt,
            briefing.Title,
            briefing.Content,
            briefing.Period,
            briefing.Actions.Select(a => new ActionItemDto(
                a.Id, a.BriefingId, a.Description, a.Type, a.Status,
                a.SuggestedAt, a.ResolvedAt, a.ResolvedBy, a.AiReasoning
            )).ToList()
        );

        return Ok(dto);
    }

    [HttpPost("generate")]
    public ActionResult<object> Generate()
    {
        // TODO: Wire up BriefingService in Fase 3
        return Ok(new { message = "Briefing generation not yet implemented" });
    }
}
