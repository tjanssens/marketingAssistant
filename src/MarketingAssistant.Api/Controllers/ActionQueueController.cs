using MarketingAssistant.Core.DTOs;
using MarketingAssistant.Core.Enums;
using MarketingAssistant.Core.Interfaces;
using MarketingAssistant.Core.Models;
using MarketingAssistant.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketingAssistant.Api.Controllers;

[ApiController]
[Route("api/actions")]
public class ActionQueueController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IActionExecutor _executor;

    public ActionQueueController(AppDbContext db, IActionExecutor executor)
    {
        _db = db;
        _executor = executor;
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
        try
        {
            var action = await _executor.ApproveAsync(id, "dashboard", ct);
            return Ok(MapToDto(action));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:int}/reject")]
    public async Task<ActionResult<ActionItemDto>> Reject(int id, CancellationToken ct)
    {
        try
        {
            var action = await _executor.RejectAsync(id, "dashboard", ct);
            return Ok(MapToDto(action));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private static ActionItemDto MapToDto(ActionItem a) => new(
        a.Id, a.BriefingId, a.Description, a.Type, a.Status,
        a.SuggestedAt, a.ResolvedAt, a.ResolvedBy, a.AiReasoning
    );
}
