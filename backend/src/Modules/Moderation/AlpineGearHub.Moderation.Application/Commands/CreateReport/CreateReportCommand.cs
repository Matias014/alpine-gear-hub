using AlpineGearHub.Moderation.Application.DTOs;
using AlpineGearHub.Moderation.Domain.Enums;
using MediatR;

namespace AlpineGearHub.Moderation.Application.Commands.CreateReport;

public record CreateReportCommand(
    Guid ListingId,
    Guid ReportedByUserId,
    ReportReason Reason,
    string? Description) : IRequest<ReportResponse>;
