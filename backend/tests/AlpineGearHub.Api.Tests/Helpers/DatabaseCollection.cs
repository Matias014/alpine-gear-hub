namespace AlpineGearHub.Api.Tests.Helpers;

// One shared factory (and its containers) for the whole test run instead of one per class -
// each Postgres/Redis container took 15+ seconds to spin up in CI, so per-class fixtures added up fast.
[CollectionDefinition(Name)]
public sealed class DatabaseCollection : ICollectionFixture<AlpineGearHubApiFactory>
{
    public const string Name = "Database";
}
