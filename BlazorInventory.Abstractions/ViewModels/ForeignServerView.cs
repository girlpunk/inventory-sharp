using MemoryPack;

namespace BlazorInventory.Abstractions.ViewModels;

/// <inheritdoc cref="BaseViewModel" />
/// <summary>
/// Represents a remote server that labels can belong to
/// </summary>
[MemoryPackable]
public sealed partial record ForeignServerView : BaseViewModel
{
    /// <summary>
    /// Namespace found in QR style labels
    /// </summary>
    public required string Namespace { get; set; }

    /// <summary>
    /// API Endpoint to contact the server to send/retrieve item details
    /// </summary>
    public Uri? Uri { get; set; }
}
