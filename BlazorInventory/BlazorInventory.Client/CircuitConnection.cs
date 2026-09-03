using ActualLab.Fusion;

namespace BlazorInventory.Client;

/// <summary>
/// Connection state of the Blazor InteractiveServer circuit (the browser's WebSocket).
/// </summary>
public enum CircuitConnectionKind
{
    // No circuit exists (static SSR page) or the circuit isn't established yet.
    None,
    Connected,
    Disconnected,
}

public record CircuitConnectionInfo(CircuitConnectionKind Kind, string? CircuitId);

public interface ICircuitConnectionState
{
    IState<CircuitConnectionInfo> State { get; }

    void Set(CircuitConnectionInfo value);
}

public sealed class CircuitConnectionState(StateFactory stateFactory) : ICircuitConnectionState
{
    private readonly MutableState<CircuitConnectionInfo> _state =
        stateFactory.NewMutable(new CircuitConnectionInfo(CircuitConnectionKind.None, null), "CircuitConnection");

    public IState<CircuitConnectionInfo> State => _state;

    public void Set(CircuitConnectionInfo value) => _state.Set(value);
}
