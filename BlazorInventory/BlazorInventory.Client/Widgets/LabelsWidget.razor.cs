using BlazorInventory.Abstractions.Models;
using Microsoft.AspNetCore.Components;

namespace BlazorInventory.Client.Widgets;

public partial class LabelsWidget
{
    /// <summary>
    /// Item to show labels for
    /// </summary>
    [Parameter]
    public Guid ItemId { get; set; }

    /// <inheritdoc />
    protected override Task<ICollection<ItemLabel>> ComputeState(CancellationToken cancellationToken) =>
        LabelService.List(ItemId, cancellationToken);
}
