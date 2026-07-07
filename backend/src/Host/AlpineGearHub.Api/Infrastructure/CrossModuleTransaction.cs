using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AlpineGearHub.Api.Infrastructure;

// A few Host-orchestrated workflows span two modules' DbContexts (e.g. Moderation resolving a
// report and then removing the underlying listing; the Stripe webhook confirming a promotion
// payment and then flipping the listing's IsPromoted flag). Each module owns a separate Postgres
// schema, but they're all the same physical database - so unlike a truly distributed system, one
// ordinary transaction spanning both DbContexts is enough to make the pair atomic. Without this,
// a crash (or a validation failure) between the two MediatR sends left one module's write
// committed and the other's not - e.g. a report resolved as "removed" while the listing itself
// stayed Active.
public static class CrossModuleTransaction
{
    public static async Task RunAsync(CancellationToken ct, Func<Task> work, params DbContext[] contexts)
    {
        if (contexts.Length < 2)
            throw new ArgumentException("At least two DbContexts are required - use a single SaveChanges otherwise.", nameof(contexts));

        var primary = contexts[0];
        await primary.Database.OpenConnectionAsync(ct);

        try
        {
            await using var transaction = await primary.Database.BeginTransactionAsync(ct);

            foreach (var context in contexts[1..])
            {
                context.Database.SetDbConnection(primary.Database.GetDbConnection());
                await context.Database.UseTransactionAsync(transaction.GetDbTransaction(), ct);
            }

            try
            {
                await work();
                await transaction.CommitAsync(ct);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }
        finally
        {
            await primary.Database.CloseConnectionAsync();
        }
    }
}
