namespace MarketingAssistant.Core.DTOs;

public record BriefingDto(
    int Id,
    DateTime GeneratedAt,
    string Title,
    string Content,
    string Period,
    List<ActionItemDto> Actions
);

public record BriefingSummaryDto(
    int Id,
    DateTime GeneratedAt,
    string Title,
    string Period,
    int ActionCount
);
