using System.ComponentModel.DataAnnotations;

namespace BlazorInventory.Abstractions.Models;

/// <summary>
/// Tag applied to an item
/// </summary>
public class ItemTag
{
    /// <summary>
    /// Item the tag is applied to
    /// </summary>
    public required Guid ItemId { get; init; }

    /// <summary>
    /// Tag value
    /// </summary>
    [MaxLength(128)]
    public required string Tag { get; set; }

    /// <summary>
    /// Item the tag is applied to
    /// </summary>
    public virtual Item Item { get; init; } = null!;
}
