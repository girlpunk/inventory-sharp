namespace InventorySharp.Models.Geolocation;

/// <inheritdoc />
/// <summary>
/// Geolocation event data. Provides the <see cref="GeolocationResult" /> with the position or error associated with the event.
/// </summary>
public sealed class GeolocationEventArgs : EventArgs
{
    /// <summary>
    /// The <see cref="GeolocationResult"/> associated with the event.
    /// </summary>
    public required GeolocationResult GeolocationResult { get; set; }
}
