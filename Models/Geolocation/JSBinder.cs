using Microsoft.JSInterop;

namespace InventorySharp.Models.Geolocation;

internal sealed class JSBinder(IJSRuntime jsRuntime, string importPath) : IAsyncDisposable
{
    private Task<IJSObjectReference>? _module;

    internal Task<IJSObjectReference> GetModule() => _module ??= jsRuntime.InvokeAsync<IJSObjectReference>("import", importPath).AsTask();

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_module != null)
        {
            var module = await _module;
            await module.DisposeAsync();
        }
    }
}