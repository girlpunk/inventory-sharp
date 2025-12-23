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

    [Inject] private ILogger<Details> Logger { get; set; }

    /// <inheritdoc />
    protected override async Task<ItemView> ComputeState(CancellationToken cancellationToken)
    {
        Logger.LogDebug("Item Details Compute Called");
        var item = await ItemService.Get(Id, cancellationToken);

        Logger.LogDebug($"Item Details Compute Completed: {item}");
        return item;
    }

    private async Task Delete()
    {
        //TODO: Verification popup

        await Commander.Run(new DeleteCommand<ItemView>
        {
            Obj = State.Value
        });

        NavigationManager.NavigateTo("/Item/");
    }

    protected override void OnAfterRender(bool firstRender)
    {
        Logger.LogDebug("Item After Render Called");
        base.OnAfterRender(firstRender);
    }
}
