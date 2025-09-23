using Microsoft.JSInterop;

namespace BlazorInventory.Client.Geolocation;

internal sealed class JSBinder(IJSRuntime jsRuntime, string importPath) : IAsyncDisposable
{
    private IJSObjectReference? _module;

    internal async Task<IJSObjectReference> GetModule() => _module ??= (await jsRuntime.InvokeAsync<IJSObjectReference>("import", importPath));

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_module != null)
        {
            await _module.DisposeAsync();
        }
    }
}
