using MemoryPack;

namespace BlazorInventory.Abstractions.ViewModels;

/// <summary>
/// Tag applied to an item
/// </summary>
[MemoryPackable]
public partial record ItemTagView
{
    /// <summary>
    /// Item the tag is applied to
    /// </summary>
    public required Guid ItemId { get; init; }

    /// <summary>
    /// Tag value
    /// </summary>
    public required string Tag { get; set; }
}
