using System.Net;
using System.Net.Http.Json;
using AlpineGearHub.Api.Endpoints;
using AlpineGearHub.Api.Tests.Helpers;
using AlpineGearHub.Identity.Application.Commands.Login;
using AlpineGearHub.Identity.Application.DTOs;
using AlpineGearHub.Listings.Application.Commands.ChangeListingStatus;
using AlpineGearHub.Listings.Application.DTOs;
using AlpineGearHub.Moderation.Application.Commands.ReviewReport;
using AlpineGearHub.Moderation.Application.DTOs;
using AlpineGearHub.Moderation.Domain.Enums;
using FluentAssertions;

namespace AlpineGearHub.Api.Tests;

[Collection(DatabaseCollection.Name)]
public sealed class ModerationTests(AlpineGearHubApiFactory factory)
{
    private async Task<ApiClient> LoginAsAdminAsync()
    {
        var admin = new ApiClient(factory.CreateClient());
        var loginResponse = await admin.PostAsync("/api/auth/login", new LoginCommand("admin@alpinegearhub.local", "Admin1234!"));
        var auth = (await loginResponse.Content.ReadFromJsonAsync<AuthResponse>())!;
        admin.SetBearerToken(auth.AccessToken);
        return admin;
    }

    [Fact]
    public async Task CreateReport_ReturnsCreatedAsPending()
    {
        var seller = await TestFlows.RegisterAsync(factory);
        var listing = await TestFlows.CreateAndPublishListingAsync(seller);
        var reporter = await TestFlows.RegisterAsync(factory);

        var response = await reporter.PostAsync("/api/moderation/reports",
            new CreateReportRequest(listing.Id, ReportReason.Counterfeit, "Fake CE certification"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var report = await response.Content.ReadFromJsonAsync<ReportResponse>();
        report!.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task GetReports_ByRegularMember_ReturnsForbidden()
    {
        var member = await TestFlows.RegisterAsync(factory);

        var response = await member.GetAsync("/api/moderation/reports");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ReviewReport_WithRemove_TakesDownTheListing()
    {
        var seller = await TestFlows.RegisterAsync(factory);
        var listing = await TestFlows.CreateAndPublishListingAsync(seller);
        var reporter = await TestFlows.RegisterAsync(factory);
        var createResponse = await reporter.PostAsync("/api/moderation/reports",
            new CreateReportRequest(listing.Id, ReportReason.Scam, "Looks fraudulent"));
        var report = (await createResponse.Content.ReadFromJsonAsync<ReportResponse>())!;

        var admin = await LoginAsAdminAsync();
        var response = await admin.PostAsync($"/api/moderation/reports/{report.Id}/review",
            new ReviewReportRequest(ReportResolution.Remove));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var reviewed = await response.Content.ReadFromJsonAsync<ReportResponse>();
        reviewed!.Status.Should().Be("Reviewed");

        var listingResponse = await seller.GetAsync($"/api/listings/{listing.Id}");
        var fetched = await listingResponse.Content.ReadFromJsonAsync<ListingResponse>();
        fetched!.Status.Should().Be("Removed");
    }

    [Fact]
    public async Task ReviewReport_WithDismiss_LeavesListingUntouched()
    {
        var seller = await TestFlows.RegisterAsync(factory);
        var listing = await TestFlows.CreateAndPublishListingAsync(seller);
        var reporter = await TestFlows.RegisterAsync(factory);
        var createResponse = await reporter.PostAsync("/api/moderation/reports",
            new CreateReportRequest(listing.Id, ReportReason.Other, "Wrong category maybe"));
        var report = (await createResponse.Content.ReadFromJsonAsync<ReportResponse>())!;

        var admin = await LoginAsAdminAsync();
        var response = await admin.PostAsync($"/api/moderation/reports/{report.Id}/review",
            new ReviewReportRequest(ReportResolution.Dismiss));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var listingResponse = await seller.GetAsync($"/api/listings/{listing.Id}");
        var fetched = await listingResponse.Content.ReadFromJsonAsync<ListingResponse>();
        fetched!.Status.Should().Be("Active");
    }

    [Fact]
    public async Task ReviewReport_WithRemove_WhenListingCannotBeRemoved_RollsBackTheReportReviewToo()
    {
        // "Remove" writes to both the Moderation and Listings schemas in one transaction (see
        // CrossModuleTransaction) - this proves that pairing is actually atomic, not just that
        // the happy path still works: a listing that's already Sold can't be Removed, so the
        // second write throws, and the report's own review must be rolled back with it rather
        // than being left stuck as "Reviewed" while the listing itself was never touched.
        var seller = await TestFlows.RegisterAsync(factory);
        var listing = await TestFlows.CreateAndPublishListingAsync(seller);
        var reporter = await TestFlows.RegisterAsync(factory);
        var createResponse = await reporter.PostAsync("/api/moderation/reports",
            new CreateReportRequest(listing.Id, ReportReason.Scam, "Looks fraudulent"));
        var report = (await createResponse.Content.ReadFromJsonAsync<ReportResponse>())!;

        await seller.PostAsync($"/api/listings/{listing.Id}/status", new ChangeStatusRequest(ListingStatusAction.Sell));

        var admin = await LoginAsAdminAsync();
        var response = await admin.PostAsync($"/api/moderation/reports/{report.Id}/review",
            new ReviewReportRequest(ReportResolution.Remove));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var reportResponse = await admin.GetAsync($"/api/moderation/reports/{report.Id}");
        var fetchedReport = await reportResponse.Content.ReadFromJsonAsync<ReportResponse>();
        fetchedReport!.Status.Should().Be("Pending");

        var listingResponse = await seller.GetAsync($"/api/listings/{listing.Id}");
        var fetchedListing = await listingResponse.Content.ReadFromJsonAsync<ListingResponse>();
        fetchedListing!.Status.Should().Be("Sold");
    }

    [Fact]
    public async Task ReviewReport_AlreadyReviewed_ReturnsUnprocessableEntity()
    {
        var seller = await TestFlows.RegisterAsync(factory);
        var listing = await TestFlows.CreateAndPublishListingAsync(seller);
        var reporter = await TestFlows.RegisterAsync(factory);
        var createResponse = await reporter.PostAsync("/api/moderation/reports",
            new CreateReportRequest(listing.Id, ReportReason.Prohibited, "desc"));
        var report = (await createResponse.Content.ReadFromJsonAsync<ReportResponse>())!;
        var admin = await LoginAsAdminAsync();
        await admin.PostAsync($"/api/moderation/reports/{report.Id}/review", new ReviewReportRequest(ReportResolution.Dismiss));

        var response = await admin.PostAsync($"/api/moderation/reports/{report.Id}/review",
            new ReviewReportRequest(ReportResolution.Remove));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
