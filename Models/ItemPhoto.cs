using System.ComponentModel.DataAnnotations;

namespace InventorySharp.Models;

/// <summary>
/// Photo of an item
/// </summary>
public class ItemPhoto : BaseModel
{
    /// <summary>
    /// Item the photo relates to
    /// </summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// Item the photo relates to
    /// </summary>
    public virtual Item Item { get; init; }

    /// <summary>
    /// Title of the photo
    /// </summary>
    [MaxLength(128)]
    public string? Title { get; set; }

    /// <summary>
    /// When the photo was uploaded
    /// </summary>
    public DateTime Uploaded { get; set; }

    /// <summary>
    /// The photo itself
    /// </summary>
    public required byte[] Data { get; set; }
}
