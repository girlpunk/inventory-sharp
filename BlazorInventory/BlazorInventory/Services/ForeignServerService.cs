using BlazorInventory.Abstractions.Models;
using BlazorInventory.Abstractions.Service;
using BlazorInventory.Data;
using Microsoft.EntityFrameworkCore;
using ActualLab.Fusion;

namespace BlazorInventory.Services;

/// <inheritdoc cref="IForeignServerService" />
public class ForeignServerService(IServiceProvider serviceProvider)
    : CRUDService<ForeignServer>(serviceProvider), IForeignServerService
{
    /// <inheritdoc />
    public override Func<ApplicationDbContext, DbSet<ForeignServer>> DbSet => static context => context.ForeignServers;

    /// <inheritdoc />
    public override void DoUpdate(ForeignServer input, ForeignServer output)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);

        output.Namespace = input.Namespace;
        output.Uri = input.Uri;
    }

    /// <inheritdoc />
    [ComputeMethod]
    public virtual async Task<Guid?> Find(string domain, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await DbHub.CreateDbContext(cancellationToken);
        return (await dbContext.ForeignServers.SingleOrDefaultAsync(i => i.Namespace == domain, cancellationToken).ConfigureAwait(false))?.Id;
    }
}
