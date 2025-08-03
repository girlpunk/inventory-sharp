using InventorySharp.Models;
using Microsoft.AspNetCore.Components;

namespace InventorySharp.Components.Widgets;

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
