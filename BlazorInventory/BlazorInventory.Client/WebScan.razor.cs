using Radzen;

namespace BlazorInventory.Client;

public partial class WebScan
{
    public async Task showModal()
    {
        await DialogService.OpenAsync<WebScanDialog>("WebScan",
            new Dictionary<string, object>(),
            new DialogOptions
            {
                Resizable = true,
                Draggable = true,
                Width = "700px",
                Height = "512px",
            });
    }
}
