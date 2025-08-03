using ActualLab.CommandR;
using InventorySharp.Commands;
using InventorySharp.Models.Geolocation;
using Microsoft.AspNetCore.Components;

namespace InventorySharp.Components;

public partial class WebScan
{
    /// <summary>
    /// Scan details
    /// </summary>
    public ScanLabelCommand Inputs { get; } = new();

    private ScanLabelResult? LastScan { get; set; }

    // private bool ShowCreate { get; set; }
    // private LabelScan? Scan { get; set; }
    // private ItemLabel? Label { get; set; }
    // private Item? Item { get; set; }
    private bool Loading { get; set; }

    private async Task Lookup()
    {
        // ShowCreate = false;
        Loading = true;
        // Scan = null;
        // Label = null;
        // Item = null;
        LastScan = null;
        StateHasChanged();

        //Get location
        var location = await GeolocationService.GetCurrentPosition(new PositionOptions()
        {
            EnableHighAccuracy = true,
            MaximumAge = 30,
        }).ConfigureAwait(false);

        if (location.IsSuccess)
        {
            Inputs.ScannerLatitude = location.Position?.Coords.Latitude;
            Inputs.ScannerLongitude = location.Position?.Coords.Longitude;
        }

        var lookupResult = await Commander.Call(Inputs).ConfigureAwait(false);

        LastScan = lookupResult;

        // if (lookupResult.Label == null)
        // {
        //     // Could not find item, offer to create?
        //     ShowCreate = true;
        //     Loading = false;
        //
        //     return;
        // }
        //
        // if (lookupResult.Scan -= null)
        // {
        //     Scan = await ScanService.Get(lookupResult.ScanId, cancellationToken)
        //     // Go to scan
        //     NavigationManager.NavigateTo($"/Scan/{lookupResult.ScanId}");
        // }
        //
        //
        // {
        //     // Go to item
        //     NavigationManager.NavigateTo($"/Label/{lookupResult.LabelId}");
        // }
    }

    /// <summary>
    /// Go to the "Create Item" page
    /// </summary>
    private void Create()
    {
        var url = NavigationManager.GetUriWithQueryParameters("/Item/Create", new Dictionary<string, object?>()
        {
            { "scanner", Inputs.ScannerId },
            { "type", Inputs.LabelType },
            { "identifier", Inputs.Identifier },
            { "latitude", Inputs.ScannerLatitude },
            { "longitude", Inputs.ScannerLongitude }
        });

        NavigationManager.NavigateTo(url);
    }
}
