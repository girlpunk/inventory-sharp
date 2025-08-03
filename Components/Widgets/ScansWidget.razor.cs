using InventorySharp.Models;
using Microsoft.AspNetCore.Components;

namespace InventorySharp.Components.Widgets;

public partial class ScansWidget
{
    /// <summary>
    /// Item to display scans for
    /// </summary>
    [Parameter]
    public Guid ItemId { get; set; }

    /// <inheritdoc />
    protected override Task<ICollection<LabelScan>> ComputeState(CancellationToken cancellationToken) =>
        ScanService.List(ItemId, cancellationToken);
}
