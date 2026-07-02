using AlpineGearHub.Moderation.Application.DTOs;
using MediatR;

namespace AlpineGearHub.Moderation.Application.Commands.ReviewReport;

public enum ReportResolution { Dismiss, Remove }

public record ReviewReportCommand(Guid ReportId, Guid ReviewerId, ReportResolution Resolution) : IRequest<ReportResponse>;
