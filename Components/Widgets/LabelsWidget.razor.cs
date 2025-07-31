using InventorySharp.Models;
using Microsoft.AspNetCore.Components;

namespace InventorySharp.Components.Widgets;

public partial class LabelsWidget
{
    [Parameter] public Guid ItemId { get; set; }

    protected override async Task<ICollection<ItemLabel>> ComputeState(CancellationToken cancellationToken) =>
        await LabelService.List(ItemId, cancellationToken);
}
