using BlazorInventory.Client.Geolocation;
using Microsoft.AspNetCore.Components;
using ActualLab.CommandR;
using BlazorInventory.Abstractions.Command;
using BlazorInventory.Abstractions.Response;

namespace BlazorInventory.Client;

public partial class WebScan
{
    bool _visible = false;

    /// <summary>
    /// Scan details
    /// </summary>
    public ScanLabelCommand Inputs { get; } = new();

    /// <summary>
    /// Show the QR code scanner
    /// </summary>
    private bool ShowScanBarcode { get; set; } = false;

    private ScanLabelResult? LastScan { get; set; }
    private bool Loading { get; set; }

    public void showModal()
    {
        _visible = true;

        // Height = "512px",
    }

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
        Loading = false;

        // if (lookupResult.Label == null)
        // {
        //     // Could not find item, offer to create?
        //     ShowCreate = true;
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

    private async Task BarcodeScan(string e)
    {
        Inputs.Identifier = e;
        ShowScanBarcode = false;
        await Lookup();
    }
}
