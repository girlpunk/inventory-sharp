using BlazorInventory.Abstractions.ViewModels;
using Microsoft.AspNetCore.Components;

namespace BlazorInventory.Client.Widgets;

public partial class ItemWidget
{
    /// <summary>
    /// Item to show details for
    /// </summary>
    [Parameter]
    [EditorRequired]
    public Guid ItemId { get; set; }

    /// <inheritdoc />
    protected override Task<ItemView> ComputeState(CancellationToken cancellationToken) =>
        ItemService.Get(ItemId, cancellationToken);
}
