using BlazorInventory.Abstractions.Service;
using BlazorInventory.Data;
using Microsoft.EntityFrameworkCore;
using ActualLab.Fusion;
using BlazorInventory.Abstractions.ViewModels;
using BlazorInventory.Data.Models;
using Mapster;

namespace BlazorInventory.Services;

/// <inheritdoc cref="IScanService" />
public class ScanService(IServiceProvider serviceProvider) : CRUDService<LabelScan, LabelScanView>(serviceProvider), IScanService
{
    /// <inheritdoc />
    public override Func<ApplicationDbContext, DbSet<LabelScan>> DbSet => static context => context.LabelScans;

    /// <inheritdoc />
    public override void DoUpdate(LabelScanView input, LabelScan output)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(input);

        output.LabelId = input.LabelId;
        output.Latitude = input.Latitude;
        output.Longitude = input.Longitude;
        output.Scanned = input.Scanned;
        output.ScannerId = input.ScannerId;
        output.ScanType = input.ScanType;

        if (input.Id != null)
            output.Id = input.Id.Value;
    }

    /// <inheritdoc />
    [ComputeMethod]
    public virtual async Task<ICollection<LabelScanView>> List(Guid itemId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await DbHub.CreateDbContext(cancellationToken);
        return await DbSet(dbContext).Where(s => s.Label.ItemId == itemId).ProjectToType<LabelScanView>().ToListAsync(cancellationToken);
    }
}
