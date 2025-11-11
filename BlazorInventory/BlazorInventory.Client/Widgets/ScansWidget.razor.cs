using BlazorInventory.Abstractions.ViewModels;
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
    protected override Task<ICollection<LabelScanView>> ComputeState(CancellationToken cancellationToken) =>
        ScanService.List(ItemId, cancellationToken);
}
