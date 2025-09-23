using ActualLab.CommandR;
using BlazorInventory.Abstractions.Command;
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
    protected override Task<Abstractions.Models.Item> ComputeState(CancellationToken cancellationToken) =>
        ItemService.Get(Id, cancellationToken);

    private async Task Delete()
    {
        //TODO: Verification popup

        await Commander.Run(new DeleteCommand<Abstractions.Models.Item>
        {
            Obj = State.Value
        }).ConfigureAwait(false);

        NavigationManager.NavigateTo("/Item/");
    }
}
