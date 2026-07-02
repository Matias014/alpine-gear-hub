using AlpineGearHub.Moderation.Application.DTOs;
using AlpineGearHub.Moderation.Application.Extensions;
using AlpineGearHub.Moderation.Domain.Repositories;
using MediatR;

namespace AlpineGearHub.Moderation.Application.Queries.GetReportById;

internal sealed class GetReportByIdQueryHandler(IReportRepository reportRepository)
    : IRequestHandler<GetReportByIdQuery, ReportResponse?>
{
    public async Task<ReportResponse?> Handle(GetReportByIdQuery request, CancellationToken cancellationToken)
    {
        var report = await reportRepository.GetByIdAsync(request.ReportId, cancellationToken);
        return report?.ToResponse();
    }
}
