using InventorySharp.Models;
using Microsoft.AspNetCore.Components;

namespace InventorySharp.Components.Pages.Label;

public partial class Details
{
    [Parameter]
    public Guid Id { get; set; }

    protected override async Task<ItemLabel> ComputeState(CancellationToken cancellationToken) =>
        await LabelService.Get(Id, cancellationToken);
}
