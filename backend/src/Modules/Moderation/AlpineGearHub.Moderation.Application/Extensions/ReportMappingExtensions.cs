using AlpineGearHub.Moderation.Application.DTOs;
using AlpineGearHub.Moderation.Domain.Entities;

namespace AlpineGearHub.Moderation.Application.Extensions;

internal static class ReportMappingExtensions
{
    public static ReportResponse ToResponse(this Report report) =>
        new(
            report.Id,
            report.ListingId,
            report.ReportedByUserId,
            report.Reason.ToString(),
            report.Description,
            report.Status.ToString(),
            report.ReviewedByUserId,
            report.ReviewedAt,
            report.CreatedAt);
}
