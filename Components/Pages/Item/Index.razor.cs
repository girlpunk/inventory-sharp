namespace InventorySharp.Components.Pages.Item;

public partial class Index
{
    /// <inheritdoc />
    protected override async Task<IList<Models.Item>> ComputeState(CancellationToken cancellationToken) =>
        (await ItemService.List(cancellationToken).ConfigureAwait(false)).ToList();
}
