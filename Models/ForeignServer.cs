using System.ComponentModel.DataAnnotations;

namespace InventorySharp.Models;

/// <summary>
/// Represents a remote server that labels can belong to
/// </summary>
public class ForeignServer : BaseModel
{
    /// <summary>
    /// Namespace found in QR style labels
    /// </summary>
    public string Namespace { get; set; }

    /// <summary>
    /// API Endpoint to contact the server to send/retrieve item details
    /// </summary>
    public Uri? Uri { get; set; }
}
