using MarketingAssistant.Core.DTOs;
using MarketingAssistant.Core.Models;
using MarketingAssistant.Infrastructure.Data;
using MarketingAssistant.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketingAssistant.Api.Controllers;

[ApiController]
[Route("api/briefings")]
public class BriefingController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly BriefingService _briefingService;

    public BriefingController(AppDbContext db, BriefingService briefingService)
    {
        _db = db;
        _briefingService = briefingService;
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

        return Ok(MapToDto(briefing));
    }

    [HttpPost("generate")]
    public async Task<ActionResult<BriefingDto>> Generate(CancellationToken ct)
    {
        var briefing = await _briefingService.GenerateAsync(ct);
        return Ok(MapToDto(briefing));
    }

    private static BriefingDto MapToDto(Briefing briefing) => new(
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
}
