using AlpineGearHub.Moderation.Application.DTOs;
using MediatR;

namespace AlpineGearHub.Moderation.Application.Queries.GetReportById;

public record GetReportByIdQuery(Guid ReportId) : IRequest<ReportResponse?>;
