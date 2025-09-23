using BlazorInventory.Abstractions.Models;
using Microsoft.AspNetCore.Components;

namespace BlazorInventory.Client.Pages.Scan;

public partial class Details
{
    /// <summary>
    /// ID of the scan being displayed
    /// </summary>
    [Parameter]
    public Guid Id { get; set; }

    /// <inheritdoc />
    protected override Task<LabelScan> ComputeState(CancellationToken cancellationToken) =>
        ScanService.Get(Id, cancellationToken);
}