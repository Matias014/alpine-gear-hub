using AlpineGearHub.Moderation.Application.DTOs;
using AlpineGearHub.Moderation.Application.Extensions;
using AlpineGearHub.Moderation.Domain.Repositories;
using MediatR;

namespace AlpineGearHub.Moderation.Application.Queries.GetReports;

internal sealed class GetReportsQueryHandler(IReportRepository reportRepository)
    : IRequestHandler<GetReportsQuery, PagedResponse<ReportResponse>>
{
    public async Task<PagedResponse<ReportResponse>> Handle(GetReportsQuery request, CancellationToken cancellationToken)
    {
        var filter = new ReportFilter(request.Status, request.Page, request.PageSize);
        var (items, totalCount) = await reportRepository.GetPagedAsync(filter, cancellationToken);

        return new PagedResponse<ReportResponse>(
            items.Select(r => r.ToResponse()).ToList(), request.Page, request.PageSize, totalCount);
    }
}
