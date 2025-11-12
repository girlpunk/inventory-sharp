using ActualLab;
using ActualLab.Collections;
using ActualLab.CommandR;
using ActualLab.Fusion;
using ActualLab.Fusion.EntityFramework.Operations;
using BlazorInventory.Abstractions.Command;
using BlazorInventory.Abstractions.Response;
using BlazorInventory.Abstractions.Service;
using BlazorInventory.Abstractions.ViewModels;
using BlazorInventory.Data;
using BlazorInventory.Data.Models;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace BlazorInventory.Services;

/// <inheritdoc cref="ILabelService" />
public class LabelService(IServiceProvider serviceProvider) : CRUDService<ItemLabel, ItemLabelView>(serviceProvider), ILabelService
{
    /// <inheritdoc />
    public override Func<ApplicationDbContext, DbSet<ItemLabel>> DbSet => static context => context.ItemLabels;

    /// <inheritdoc />
    public override void DoUpdate(ItemLabelView input, ItemLabel output)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(input);

        output.ItemId = input.ItemId;
        output.Created = input.Created;
        output.ForeignServerId = input.ForeignServerId;
        output.Identifier = input.Identifier;
        output.LabelType = input.LabelType;

        if (input.Id != null)
            output.Id = input.Id.Value;
    }

    /// <inheritdoc />
    public virtual async Task<ScanLabelResult> Scan(ScanLabelCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var context = CommandContext.GetCurrent();
        DbOperationScope<ApplicationDbContext>.GetOrCreate(context).Require();

        if (Invalidation.IsActive)
        {
            if (!command.CreateScanRecord) return null!;

            var id = context.Items.Get<Guid?>("id", null);

            if (id == null)
                return null!;

            _ = Get(id.Value, CancellationToken.None);
            _ = List(id.Value, CancellationToken.None);

            return null!;
        }

        await using var dbContext = await DbHub.CreateOperationDbContext(cancellationToken);

        ItemLabel? label;

        if (command.LabelId != null)
        {
            label = await dbContext.ItemLabels
                .Include(static l => l.Item).ThenInclude(static item => item.Parent)
                .SingleOrDefaultAsync(l => l.Id == command.LabelId, cancellationToken);
        }
        else
        {
            ArgumentNullException.ThrowIfNull(command.Identifier);

            if (command.Identifier.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                command.Identifier.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                //https://r.lu.gl/a/A808AZ

                var url = new Uri(command.Identifier, UriKind.Absolute);

                label = await dbContext.ItemLabels
                    .Include(static l => l.Item)
                    .ThenInclude(static item => item.Parent)
                    .SingleOrDefaultAsync(l =>
                        (l.ForeignServer == null && l.Identifier == command.Identifier) ||
                        (l.ForeignServer != null && l.ForeignServer.Namespace == url.Host &&
                         l.Identifier == url.AbsolutePath), cancellationToken);
            }
            else
            {
                label = await dbContext.ItemLabels
                    .Include(static l => l.Item)
                    .ThenInclude(static item => item.Parent)
                    .SingleOrDefaultAsync(l => l.ForeignServer == null && l.Identifier == command.Identifier, cancellationToken);
            }
        }

        var result = new ScanLabelResult();

        if (label == null)
            return result;

        result.Label = label.Adapt<ItemLabelView>();
        result.Item = label.Item.Adapt<ItemView>();
        result.ParentItem = label.Item.Parent?.Adapt<ItemView>();

        if (!command.CreateScanRecord)
            return result;

        var scanner = await dbContext.Scanners.SingleOrDefaultAsync(s => s.Id == command.ScannerId, cancellationToken);

        var scanRecord = new LabelScan()
        {
            ScannerId = command.ScannerId,
            LabelId = label.Id,
            ScanType = command.LabelType,
            Scanned = DateTimeOffset.Now,
            Latitude = command.ScannerLatitude ?? scanner?.Latitude,
            Longitude = command.ScannerLongitude ?? scanner?.Longitude,
        };

        dbContext.LabelScans.Add(scanRecord);

        if (scanner?.ParentItemId != null)
            await context.Commander.Call(new SetItemParentCommand
            {
                ItemId = label.ItemId,
                ParentId = scanner.ParentItemId.Value
            }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        result.Scan = scanRecord.Adapt<LabelScanView>();
        result.Scanner = scanner?.Adapt<ScannerView>();

        return result;
    }

    /// <inheritdoc />
    [ComputeMethod]
    public virtual async Task<ICollection<ItemLabelView>> List(Guid itemId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await DbHub.CreateDbContext(cancellationToken);
        return await DbSet(dbContext).Where(l => l.ItemId == itemId).ProjectToType<ItemLabelView>().ToListAsync(cancellationToken);
    }
}
