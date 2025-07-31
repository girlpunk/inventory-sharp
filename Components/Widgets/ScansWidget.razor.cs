using InventorySharp.Models;
using Microsoft.AspNetCore.Components;

namespace InventorySharp.Components.Widgets;

public partial class ScansWidget
{
    [Parameter] public Guid ItemId { get; set; }

    /// <inheritdoc />
    protected override async Task<ICollection<LabelScan>> ComputeState(CancellationToken cancellationToken) =>
        await ScanService.List(ItemId, cancellationToken);
}
