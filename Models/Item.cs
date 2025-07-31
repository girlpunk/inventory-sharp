using System.ComponentModel.DataAnnotations;

namespace InventorySharp.Models;

/// <summary>
/// Item that can be tracked
/// </summary>
public class Item : BaseModel
{
    /// <summary>
    /// Name of the item
    /// </summary>
    [MaxLength(120)]
    public string? Name { get; set; }

    /// <summary>
    /// Description of the item
    /// </summary>
    [MaxLength(1024)]
    public string? Description { get; set; }

    /// <summary>
    /// ID of the item's parent, or null for no parent
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// When the item was created
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Photos associated with the item
    /// </summary>
    public virtual ICollection<ItemPhoto> Photos { get; init; } = null!;

    /// <summary>
    /// Parent item
    /// </summary>
    public virtual Item? Parent { get; init; }

    /// <summary>
    /// Labels applied to the item
    /// </summary>
    public virtual ICollection<ItemLabel> Labels { get; init; } = null!;

    /// <summary>
    /// Tags applied to the item
    /// </summary>
    public virtual ICollection<ItemTag> Tags { get; init; } = null!;
}
