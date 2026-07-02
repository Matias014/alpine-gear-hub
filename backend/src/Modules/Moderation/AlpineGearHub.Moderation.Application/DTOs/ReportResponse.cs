namespace AlpineGearHub.Moderation.Application.DTOs;

public record ReportResponse(
    Guid Id,
    Guid ListingId,
    Guid ReportedByUserId,
    string Reason,
    string? Description,
    string Status,
    Guid? ReviewedByUserId,
    DateTime? ReviewedAt,
    DateTime CreatedAt);
