using Microsoft.AspNetCore.Components;

namespace InventorySharp.Components.Widgets;

public partial class ItemWidget
{
    /// <summary>
    /// Item to show details for
    /// </summary>
    [Parameter]
    public Guid ItemId { get; set; }

    /// <inheritdoc />
    protected override Task<Models.Item> ComputeState(CancellationToken cancellationToken) =>
        ItemService.Get(ItemId, cancellationToken);
}
