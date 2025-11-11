using MemoryPack;

namespace BlazorInventory.Abstractions.ViewModels;

/// <inheritdoc />
/// <summary>
/// Item that can be tracked
/// </summary>
[MemoryPackable]
public partial record ItemView : BaseViewModel
{
    /// <summary>
    /// Name of the item
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Description of the item
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// ID of the item's parent, or null for no parent
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// When the item was created
    /// </summary>
    public DateTimeOffset Created { get; set; }

    /// <summary>
    /// Photos associated with the item
    /// </summary>
    public ItemPhotoView[]? Photos { get; init; } = null!;

    /// <summary>
    /// Labels applied to the item
    /// </summary>
    public ItemLabelView[]? Labels { get; init; } = null!;

    /// <summary>
    /// Tags applied to the item
    /// </summary>
    public ItemTagView[]? Tags { get; init; } = null!;
}
