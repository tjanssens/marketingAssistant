using MarketingAssistant.Core.Enums;

namespace MarketingAssistant.Core.DTOs;

public record ActionItemDto(
    int Id,
    int? BriefingId,
    string Description,
    ActionType Type,
    ActionStatus Status,
    DateTime SuggestedAt,
    DateTime? ResolvedAt,
    string? ResolvedBy,
    string AiReasoning
);
