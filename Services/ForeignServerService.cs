using InventorySharp.Models;
using Microsoft.EntityFrameworkCore;

namespace InventorySharp.Services;

/// <inheritdoc />
public class ForeignServerService(IServiceProvider serviceProvider)
    : CRUDService<ForeignServer>(serviceProvider), IForeignServerService
{
    /// <inheritdoc />
    public override Func<AppDbContext, DbSet<ForeignServer>> DbSet => static context => context.ForeignServers;

    /// <inheritdoc />
    public override void DoUpdate(ForeignServer input, ForeignServer output)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);

        output.Namespace = input.Namespace;
        output.Uri = input.Uri;
    }

    /// <inheritdoc />
    public async Task<Guid?> Find(string domain, CancellationToken cancellationToken=default)
    {
        await using var dbContext = await DbHub.CreateDbContext(cancellationToken);
        return (await dbContext.ForeignServers.SingleOrDefaultAsync(i => i.Namespace == domain, cancellationToken))?.Id;
    }
}
