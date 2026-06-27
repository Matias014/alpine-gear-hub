using AlpineGearHub.SharedKernel;

namespace AlpineGearHub.Listings.Domain.Entities;

public class Category : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;

    private Category() { }

    public static Category Create(string name, string slug) =>
        new() { Id = Guid.NewGuid(), Name = name, Slug = slug };
}
