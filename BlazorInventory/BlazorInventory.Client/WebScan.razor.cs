using BlazorInventory.Client.Geolocation;
using Microsoft.AspNetCore.Components;
using ActualLab.CommandR;
using BlazorInventory.Abstractions.Command;
using BlazorInventory.Abstractions.Models;
using BlazorInventory.Abstractions.Response;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorInventory.Client;

public sealed partial class WebScan
{
    private bool _visible;

    /// <summary>
    /// Scan details
    /// </summary>
    public ScanLabelCommand Inputs { get; set; } = new();

    /// <summary>
    /// Show the QR code scanner
    /// </summary>
    private bool ShowScanBarcode { get; set; }

    private ScanLabelResult? LastScan { get; set; }
    private bool Loading { get; set; }

    private void ShowModal()
    {
        Inputs = new ScanLabelCommand();
        _visible = true;
    }

    private async Task Lookup(EditContext? editContext)
    {
        if (Loading)
            return;

        try
        {
            Loading = true;
            LastScan = null;
            await InvokeAsync(StateHasChanged);

            //Get location
            var location = await GeolocationService.GetCurrentPosition(new PositionOptions()
            {
                EnableHighAccuracy = true,
                MaximumAge = 30,
            });

            if (location.IsSuccess)
            {
                Inputs.ScannerLatitude = location.Position?.Coords.Latitude;
                Inputs.ScannerLongitude = location.Position?.Coords.Longitude;
            }
            else
            {
                location = await GeolocationService.GetCurrentPosition(new PositionOptions()
                {
                    EnableHighAccuracy = false,
                    MaximumAge = 30,
                });

                if (location.IsSuccess)
                {
                    Inputs.ScannerLatitude = location.Position?.Coords.Latitude;
                    Inputs.ScannerLongitude = location.Position?.Coords.Longitude;
                }
            }

            var lookupResult = await Commander.Call(Inputs);
            ArgumentNullException.ThrowIfNull(lookupResult);

            LastScan = lookupResult;
        }
        finally
        {
            Loading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    /// <summary>
    /// Go to the "Create Item" page
    /// </summary>
    private void Create()
    {
        var parameters = new Dictionary<string, object?>()
        {
            { "identifier", Inputs.Identifier }
        };

        if (Inputs.ScannerId != null)
            parameters["scanner"] = Inputs.ScannerId;

        if (Inputs.LabelType != LabelType.None)
            parameters["type"] = Inputs.LabelType;

        if (Inputs is { ScannerLatitude: not null, ScannerLongitude: not null })
        {
            parameters["latitude"] = Inputs.ScannerLatitude;
            parameters["longitude"] = Inputs.ScannerLongitude;
        }

        var url = NavigationManager.GetUriWithQueryParameters("/Item/Create", parameters);

        NavigationManager.NavigateTo(url);
    }

    private void Link()
    {
        var parameters = new Dictionary<string, object?>()
        {
            { "identifier", Inputs.Identifier }
        };

        if (Inputs.ScannerId != null)
            parameters["scanner"] = Inputs.ScannerId;

        if (Inputs.LabelType != LabelType.None)
            parameters["type"] = Inputs.LabelType;

        if (Inputs is { ScannerLatitude: not null, ScannerLongitude: not null })
        {
            parameters["latitude"] = Inputs.ScannerLatitude;
            parameters["longitude"] = Inputs.ScannerLongitude;
        }

        var url = NavigationManager.GetUriWithQueryParameters("/Item/Create", parameters);

        NavigationManager.NavigateTo(url);
    }

    private async Task BarcodeScan(string e)
    {
        Inputs.Identifier = e;
        ShowScanBarcode = false;
        await Lookup(null);
    }
}
