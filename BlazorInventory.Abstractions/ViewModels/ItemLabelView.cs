using BlazorInventory.Abstractions.Models;
using MemoryPack;

namespace BlazorInventory.Abstractions.ViewModels;

/// <inheritdoc />
/// <summary>
/// Label attached to an item
/// </summary>
[MemoryPackable]
public partial record ItemLabelView : BaseViewModel
{
    /// <summary>
    /// Item the label is associated with
    /// </summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// Identifier shown on the label
    /// </summary>
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
    public ForeignServerView? ForeignServer { get; init; }

    /// <summary>
    /// When the label was created
    /// </summary>
    public DateTimeOffset Created { get; set; }
}