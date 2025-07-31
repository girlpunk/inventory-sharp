using Microsoft.AspNetCore.Components;

namespace InventorySharp.Components.Widgets;

public partial class ItemWidget
{
    [Parameter] public Guid ItemId { get; set; }

    /// <inheritdoc />
    protected override async Task<Models.Item> ComputeState(CancellationToken cancellationToken) =>
        await ItemService.Get(ItemId, cancellationToken);
}
