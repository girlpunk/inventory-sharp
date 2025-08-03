using ActualLab.CommandR;
using InventorySharp.Commands;
using Microsoft.AspNetCore.Components;

namespace InventorySharp.Components.Pages.Item;

public sealed partial class Details
{
    /// <summary>
    /// ID of the item to show the details for
    /// </summary>
    [Parameter]
    public Guid Id { get; set; }

    /// <inheritdoc />
    protected override Task<Models.Item> ComputeState(CancellationToken cancellationToken) =>
        ItemService.Get(Id, cancellationToken);

    private async Task Delete()
    {
        //TODO: Verification popup

        await Commander.Run(new DeleteCommand<Models.Item>
        {
            Obj = State.Value
        }).ConfigureAwait(false);

        NavigationManager.NavigateTo("/Item/");
    }
}
