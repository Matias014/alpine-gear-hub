using AlpineGearHub.Moderation.Application.DTOs;
using AlpineGearHub.Moderation.Application.Extensions;
using AlpineGearHub.Moderation.Domain.Repositories;
using MediatR;

namespace AlpineGearHub.Moderation.Application.Commands.ReviewReport;

internal sealed class ReviewReportCommandHandler(IReportRepository reportRepository)
    : IRequestHandler<ReviewReportCommand, ReportResponse>
{
    public async Task<ReportResponse> Handle(ReviewReportCommand request, CancellationToken cancellationToken)
    {
        var report = await reportRepository.GetByIdAsync(request.ReportId, cancellationToken)
            ?? throw new InvalidOperationException($"Report '{request.ReportId}' not found.");

        if (request.Resolution == ReportResolution.Remove)
            report.MarkReviewed(request.ReviewerId);
        else
            report.Dismiss(request.ReviewerId);

        await reportRepository.SaveChangesAsync(cancellationToken);

        return report.ToResponse();
    }
}
