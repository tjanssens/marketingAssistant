using MarketingAssistant.Core.DTOs;
using MarketingAssistant.Core.Enums;
using MarketingAssistant.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketingAssistant.Api.Controllers;

[ApiController]
[Route("api/actions")]
public class ActionQueueController : ControllerBase
{
    private readonly AppDbContext _db;

    public ActionQueueController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<ActionItemDto>>> GetAll([FromQuery] ActionStatus? status, CancellationToken ct)
    {
        var query = _db.ActionItems.AsQueryable();

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        var actions = await query
            .OrderByDescending(a => a.SuggestedAt)
            .Select(a => new ActionItemDto(
                a.Id, a.BriefingId, a.Description, a.Type, a.Status,
                a.SuggestedAt, a.ResolvedAt, a.ResolvedBy, a.AiReasoning
            ))
            .Take(100)
            .ToListAsync(ct);

        return Ok(actions);
    }

    [HttpPost("{id:int}/approve")]
    public async Task<ActionResult<ActionItemDto>> Approve(int id, CancellationToken ct)
    {
        var action = await _db.ActionItems.FindAsync([id], ct);
        if (action is null)
            return NotFound();

        if (action.Status != ActionStatus.Pending)
            return BadRequest(new { error = "Action is not in Pending status" });

        action.Status = ActionStatus.Approved;
        action.ResolvedAt = DateTime.UtcNow;
        action.ResolvedBy = "dashboard";
        await _db.SaveChangesAsync(ct);

        return Ok(new ActionItemDto(
            action.Id, action.BriefingId, action.Description, action.Type, action.Status,
            action.SuggestedAt, action.ResolvedAt, action.ResolvedBy, action.AiReasoning
        ));
    }

    [HttpPost("{id:int}/reject")]
    public async Task<ActionResult<ActionItemDto>> Reject(int id, CancellationToken ct)
    {
        var action = await _db.ActionItems.FindAsync([id], ct);
        if (action is null)
            return NotFound();

        if (action.Status != ActionStatus.Pending)
            return BadRequest(new { error = "Action is not in Pending status" });

        action.Status = ActionStatus.Rejected;
        action.ResolvedAt = DateTime.UtcNow;
        action.ResolvedBy = "dashboard";
        await _db.SaveChangesAsync(ct);

        return Ok(new ActionItemDto(
            action.Id, action.BriefingId, action.Description, action.Type, action.Status,
            action.SuggestedAt, action.ResolvedAt, action.ResolvedBy, action.AiReasoning
        ));
    }
}
