using ActualLab.CommandR;
using InventorySharp.Commands;
using Microsoft.AspNetCore.Components;

namespace InventorySharp.Components.Pages.Item;

public partial class Details
{
    [Parameter]
    public Guid Id { get; set; }

    protected override async Task<Models.Item> ComputeState(CancellationToken cancellationToken) =>
        await ItemService.Get(Id, cancellationToken);

    private async Task Delete()
    {
        //TODO: Verification popup

        await Commander.Run(new DeleteCommand<Models.Item>()
        {
            Obj = State.Value
        });

        NavigationManager.NavigateTo("/Item/");
    }
}
