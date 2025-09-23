using BlazorInventory.Abstractions.Models;
using Microsoft.AspNetCore.Components;

namespace BlazorInventory.Client.Widgets;

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
