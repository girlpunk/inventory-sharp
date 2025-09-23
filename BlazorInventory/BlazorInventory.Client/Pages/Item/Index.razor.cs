namespace BlazorInventory.Client.Pages.Item;

public partial class Index
{
    /// <inheritdoc />
    protected override async Task<IList<Abstractions.Models.Item>> ComputeState(CancellationToken cancellationToken) =>
        (await ItemService.List(cancellationToken).ConfigureAwait(false)).ToList();
}
