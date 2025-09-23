using Microsoft.AspNetCore.Components;

namespace BlazorInventory.Client.Widgets;

public partial class ItemWidget
{
    /// <summary>
    /// Item to show details for
    /// </summary>
    [Parameter]
    public Guid ItemId { get; set; }

    /// <inheritdoc />
    protected override Task<Abstractions.Models.Item> ComputeState(CancellationToken cancellationToken) =>
        ItemService.Get(ItemId, cancellationToken);
}
