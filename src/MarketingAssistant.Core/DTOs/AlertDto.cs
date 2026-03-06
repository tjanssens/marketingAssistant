using MarketingAssistant.Core.Enums;

namespace MarketingAssistant.Core.DTOs;

public record AlertDto(
    int Id,
    DateTime CreatedAt,
    AlertSeverity Severity,
    string Title,
    string Message,
    string Category,
    bool IsAcknowledged
);
