using BlazorInventory.Abstractions.Models;
using BlazorInventory.Abstractions.Service;
using BlazorInventory.Data;
using Microsoft.EntityFrameworkCore;

namespace BlazorInventory.Services;

/// <inheritdoc cref="IScanService" />
public class ScanService(IServiceProvider serviceProvider) : CRUDService<LabelScan>(serviceProvider), IScanService
{
    /// <inheritdoc />
    public override Func<ApplicationDbContext, DbSet<LabelScan>> DbSet => static context => context.LabelScans;

    /// <inheritdoc />
    public override void DoUpdate(LabelScan input, LabelScan output)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(input);

        output.LabelId = input.LabelId;
        output.Latitude = input.Latitude;
        output.Longitude = input.Longitude;
        output.Scanned = input.Scanned;
        output.ScannerId = input.ScannerId;
        output.ScanType = input.ScanType;
        output.Id = input.Id;
    }

    /// <inheritdoc />
    public virtual async Task<ICollection<LabelScan>> List(Guid itemId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await DbHub.CreateDbContext(cancellationToken);
        return await DbSet(dbContext).Where(s => s.Label.ItemId == itemId).ToListAsync(cancellationToken).ConfigureAwait(false);
    }
}
