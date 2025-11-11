using ActualLab.CommandR;
using BlazorInventory.Abstractions.Command;
using BlazorInventory.Abstractions.ViewModels;
using Microsoft.AspNetCore.Components;

namespace BlazorInventory.Client.Pages.Item;

public sealed partial class Details
{
    /// <summary>
    /// ID of the item to show the details for
    /// </summary>
    [Parameter]
    public Guid Id { get; set; }

    /// <inheritdoc />
    protected override Task<ItemView> ComputeState(CancellationToken cancellationToken) =>
        ItemService.Get(Id, cancellationToken);

    private async Task Delete()
    {
        //TODO: Verification popup

        await Commander.Run(new DeleteCommand<ItemView>
        {
            Obj = State.Value
        });

        NavigationManager.NavigateTo("/Item/");
    }
}
