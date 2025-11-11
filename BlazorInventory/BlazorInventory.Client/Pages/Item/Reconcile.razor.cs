using ActualLab.Collections;
using BlazorInventory.Abstractions.ViewModels;
using Microsoft.AspNetCore.Components;

namespace BlazorInventory.Client.Pages.Item;

public partial class Reconcile
{
    private ICollection<ItemView> OutstandingItems { get; } = [];
    private ICollection<ItemView> CompletedItems { get; } = [];
    private ItemView? Item { get; set; }

    /// <summary>
    /// ID of the item to reconcile the contents of
    /// </summary>
    [Parameter]
    public Guid? Id { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        if (Item == null && Id != null)
        {
            Item = await ItemService.Get(Id.Value);
            OutstandingItems.AddRange(await ItemService.ListChildren(Id.Value));
        }
    }
}