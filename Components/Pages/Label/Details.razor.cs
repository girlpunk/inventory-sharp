using InventorySharp.Models;
using Microsoft.AspNetCore.Components;

namespace InventorySharp.Components.Pages.Label;

public partial class Details
{
    /// <summary>
    /// ID of the label being displayed
    /// </summary>
    [Parameter]
    public Guid Id { get; set; }

    /// <inheritdoc />
    protected override Task<ItemLabel> ComputeState(CancellationToken cancellationToken) =>
        LabelService.Get(Id, cancellationToken);
}
