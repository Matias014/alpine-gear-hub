using AlpineGearHub.Moderation.Application.DTOs;
using AlpineGearHub.Moderation.Domain.Enums;
using MediatR;

namespace AlpineGearHub.Moderation.Application.Queries.GetReports;

public record GetReportsQuery(
    ReportStatus? Status = null,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResponse<ReportResponse>>;
