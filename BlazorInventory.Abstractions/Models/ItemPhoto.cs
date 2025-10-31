using System.ComponentModel.DataAnnotations;

namespace BlazorInventory.Abstractions.Models;

/// <inheritdoc />
/// <summary>
/// Photo of an item
/// </summary>
public class ItemPhoto : BaseModel
{
    /// <summary>
    /// Item the photo relates to
    /// </summary>
    public Guid ItemId { get; init; }

    /// <summary>
    /// Item the photo relates to
    /// </summary>
    public virtual Item Item { get; init; } = null!;

    /// <summary>
    /// Title of the photo
    /// </summary>
    [MaxLength(128)]
    public string? Title { get; set; }

    /// <summary>
    /// When the photo was uploaded
    /// </summary>
    public DateTime Uploaded { get; init; }

    /// <summary>
    /// The photo itself
    /// </summary>
    public required byte[] Data { get; init; }
}
