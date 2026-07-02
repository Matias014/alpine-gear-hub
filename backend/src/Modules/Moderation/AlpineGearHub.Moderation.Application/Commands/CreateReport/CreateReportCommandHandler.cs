using AlpineGearHub.Moderation.Application.DTOs;
using AlpineGearHub.Moderation.Application.Extensions;
using AlpineGearHub.Moderation.Domain.Entities;
using AlpineGearHub.Moderation.Domain.Repositories;
using MediatR;

namespace AlpineGearHub.Moderation.Application.Commands.CreateReport;

internal sealed class CreateReportCommandHandler(IReportRepository reportRepository)
    : IRequestHandler<CreateReportCommand, ReportResponse>
{
    public async Task<ReportResponse> Handle(CreateReportCommand request, CancellationToken cancellationToken)
    {
        var report = Report.Create(request.ListingId, request.ReportedByUserId, request.Reason, request.Description);

        await reportRepository.AddAsync(report, cancellationToken);
        await reportRepository.SaveChangesAsync(cancellationToken);

        return report.ToResponse();
    }
}
