namespace InventorySharp.Models.Geolocation;

/// <summary>
/// An enumeration of error codes used by <see cref="GeolocationPositionError"/>.
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GeolocationPositionError/code"/>.
/// </summary>
public enum GeolocationPositionErrorCode
{
    /// <summary>
    /// Geolocation failed because the device does not support geolocation. Not part of W3C spec.
    /// </summary>
    DeviceNotSupported = 0,

    /// <summary>
    /// Geolocation failed because permission to access location was denied.
    /// </summary>
    PermissionDenied = 1,

    /// <summary>
    /// Geolocation failed because of an internal error on the device.
    /// </summary>
    PositionUnavailable = 2,

    /// <summary>
    /// Geolocation failed because no position was returned in time.
    /// </summary>
    Timeout = 3
}