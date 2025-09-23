using System.ComponentModel.DataAnnotations;

namespace BlazorInventory.Abstractions.Models;

/// <inheritdoc />
/// <summary>
/// Represents a remote server that labels can belong to
/// </summary>
public sealed class ForeignServer : BaseModel
{
    /// <summary>
    /// Namespace found in QR style labels
    /// </summary>
    [MaxLength(255)]
    public required string Namespace { get; set; }

    /// <summary>
    /// API Endpoint to contact the server to send/retrieve item details
    /// </summary>
    public Uri? Uri { get; set; }
}
