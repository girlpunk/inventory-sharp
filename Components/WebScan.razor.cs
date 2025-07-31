using ActualLab.CommandR;
using InventorySharp.Commands;
using InventorySharp.Models.Geolocation;
using Microsoft.AspNetCore.Components;

namespace InventorySharp.Components;

public partial class WebScan
{
    public ScanLabelCommand Inputs { get; set; } = new();

    private bool ShowCreate { get; set; }
    private bool Loading { get; set; }

    [Inject]
    public ICommander Commander { get; set; } = null!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;

    [Inject] public IGeolocationService GeolocationService { get; set; } = null!;

    private async Task Lookup()
    {
        ShowCreate = false;
        Loading = true;
        StateHasChanged();

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

        var lookupResult = await Commander.Call(Inputs);

        if (lookupResult.LabelId == null)
        {
            // Could not find item, offer to create?
            ShowCreate = true;
            Loading = false;

            return;
        }

        if (lookupResult.ScanId != null)
        {
            // Go to scan
            NavigationManager.NavigateTo($"/Scan/{lookupResult.ScanId}");
        }
        else
        {
            // Go to item
            NavigationManager.NavigateTo($"/Label/{lookupResult.LabelId}");
        }
    }

    public void Create()
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
