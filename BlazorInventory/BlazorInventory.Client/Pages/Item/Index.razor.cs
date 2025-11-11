using BlazorInventory.Abstractions.ViewModels;

namespace BlazorInventory.Client.Pages.Item;

public partial class Index
{
    /// <inheritdoc />
    protected override async Task<IList<ItemView>> ComputeState(CancellationToken cancellationToken) =>
        (await ItemService.List(cancellationToken)).ToList();
}
