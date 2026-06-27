using AlpineGearHub.Listings.Application.Queries.GetCategories;
using MediatR;

namespace AlpineGearHub.Api.Endpoints;

public static class CategoryEndpoints
{
    public static RouteGroupBuilder MapCategoryEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetCategoriesQuery(), ct);
            return Results.Ok(result);
        })
        .AllowAnonymous()
        .WithSummary("Get all categories");

        return group;
    }
}
