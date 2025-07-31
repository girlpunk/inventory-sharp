using System.ComponentModel.DataAnnotations;

namespace InventorySharp.Models;

public class ItemLabel : BaseModel
{
    /// <summary>
    /// Item the label is associated with
    /// </summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// Item the label is associated with
    /// </summary>
    public virtual Item Item { get; set; }

    /// <summary>
    /// Identifier shown on the label
    /// </summary>
    [MaxLength(256)]
    public required string Identifier { get; set; }

    /// <summary>
    /// What type of label this is
    /// </summary>
    public required LabelType LabelType { get; set; }

    /// <summary>
    /// ID of the foreign server the item belongs to, or null for no foreign server
    /// </summary>
    public Guid? ForeignServerId { get; set; }

    /// <summary>
    /// ID of the foreign server the item belongs to, or null for no foreign server
    /// </summary>
    public virtual ForeignServer? ForeignServer { get; init; }

    /// <summary>
    /// When the label was created
    /// </summary>
    public DateTime Created { get; set; }
}
