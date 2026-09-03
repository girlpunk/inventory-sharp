using BlazorInventory.Client;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Logging;

namespace BlazorInventory.Services;

/// <summary>
/// Tracks the state of the Blazor InteractiveServer circuit (the browser's WebSocket)
/// and publishes it to <see cref="ICircuitConnectionState"/> for the UI.
/// </summary>
public sealed class CircuitConnectionHandler(
    ICircuitConnectionState circuitState,
    ILogger<CircuitConnectionHandler> logger)
    : CircuitHandler
{
    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        logger.LogDebug("Circuit {CircuitId} opened", circuit.Id);
        return Task.CompletedTask;
    }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        var wasDown = circuitState.State.Value.Kind == CircuitConnectionKind.Disconnected;
        circuitState.Set(new(CircuitConnectionKind.Connected, circuit.Id));
        if (wasDown)
            logger.LogInformation("Circuit {CircuitId} reconnected after the connection was dropped", circuit.Id);
        else
            logger.LogDebug("Circuit {CircuitId} connection established", circuit.Id);
        return Task.CompletedTask;
    }

    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        circuitState.Set(new(CircuitConnectionKind.Disconnected, circuit.Id));
        logger.LogInformation("Circuit {CircuitId} connection dropped, client will attempt to reconnect", circuit.Id);
        return Task.CompletedTask;
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        circuitState.Set(new(CircuitConnectionKind.None, null));
        logger.LogDebug("Circuit {CircuitId} closed (evicted)", circuit.Id);
        return Task.CompletedTask;
    }
}
