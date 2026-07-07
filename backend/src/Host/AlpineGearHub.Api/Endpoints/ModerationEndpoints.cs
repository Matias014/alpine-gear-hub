using System.Security.Claims;
using AlpineGearHub.Api.Infrastructure;
using AlpineGearHub.Listings.Application.Commands.ChangeListingStatus;
using AlpineGearHub.Listings.Application.Queries.GetListingById;
using AlpineGearHub.Listings.Infrastructure.Data;
using AlpineGearHub.Moderation.Application.Commands.CreateReport;
using AlpineGearHub.Moderation.Application.Commands.ReviewReport;
using AlpineGearHub.Moderation.Application.DTOs;
using AlpineGearHub.Moderation.Application.Queries.GetReportById;
using AlpineGearHub.Moderation.Application.Queries.GetReports;
using AlpineGearHub.Moderation.Domain.Enums;
using AlpineGearHub.Moderation.Infrastructure.Data;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AlpineGearHub.Api.Endpoints;

public static class ModerationEndpoints
{
    public static RouteGroupBuilder MapModerationEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/reports", async (
            ISender sender,
            CreateReportRequest body,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            // The Moderation module never references Listings directly, so existence is
            // checked here in the Host by querying the Listings module first.
            var listing = await sender.Send(new GetListingByIdQuery(body.ListingId), ct);
            if (listing is null) return Results.NotFound();

            var reporterId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await sender.Send(
                new CreateReportCommand(body.ListingId, reporterId, body.Reason, body.Description), ct);
            return Results.Created($"/api/moderation/reports/{result.Id}", result);
        })
        .RequireAuthorization()
        .WithSummary("Report a listing");

        group.MapGet("/reports", async (
            ISender sender,
            [FromQuery] ReportStatus? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default) =>
        {
            var result = await sender.Send(new GetReportsQuery(status, page, pageSize), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization("RequireModerator")
        .WithSummary("Get reports (moderation queue)");

        group.MapGet("/reports/{id:guid}", async (ISender sender, Guid id, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetReportByIdQuery(id), ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .RequireAuthorization("RequireModerator")
        .WithSummary("Get a report by id");

        group.MapPost("/reports/{id:guid}/review", async (
            ISender sender,
            ModerationDbContext moderationDb,
            ListingsDbContext listingsDb,
            Guid id,
            ReviewReportRequest body,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var reviewerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            ReportResponse? report = null;

            // "Remove" both resolves the report and removes the underlying listing, via the
            // same status-change path a seller would use — Moderation just supplies the trigger.
            // Both writes share one transaction so a failure partway through can't leave the
            // report resolved while the listing itself stays Active (or vice versa).
            if (body.Resolution == ReportResolution.Remove)
            {
                await CrossModuleTransaction.RunAsync(ct, async () =>
                {
                    report = await sender.Send(new ReviewReportCommand(id, reviewerId, body.Resolution), ct);
                    await sender.Send(
                        new ChangeListingStatusCommand(report.ListingId, reviewerId, true, ListingStatusAction.Remove), ct);
                }, moderationDb, listingsDb);
            }
            else
            {
                report = await sender.Send(new ReviewReportCommand(id, reviewerId, body.Resolution), ct);
            }

            return Results.Ok(report);
        })
        .RequireAuthorization("RequireModerator")
        .WithSummary("Review a report (dismiss, or remove the listing)");

        return group;
    }
}

public record CreateReportRequest(Guid ListingId, ReportReason Reason, string? Description);
public record ReviewReportRequest(ReportResolution Resolution);
