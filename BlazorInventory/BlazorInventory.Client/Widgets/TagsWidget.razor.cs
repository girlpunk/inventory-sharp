using BlazorInventory.Abstractions.ViewModels;
using Microsoft.AspNetCore.Components;

namespace BlazorInventory.Client.Widgets;

public partial class TagsWidget
{
    /// <summary>
    /// Item to show tags for
    /// </summary>
    [Parameter]
    [EditorRequired]
    public Guid ItemId { get; set; }

    /// <inheritdoc />
    protected override Task<ICollection<ItemTagView>> ComputeState(CancellationToken cancellationToken) =>
        TagService.List(ItemId, cancellationToken);
}