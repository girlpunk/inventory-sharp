using MemoryPack;

namespace BlazorInventory.Abstractions.ViewModels;

/// <inheritdoc />
/// <summary>
/// Photo of an item
/// </summary>
[MemoryPackable]
public partial record ItemPhotoView : BaseViewModel
{
    /// <summary>
    /// Item the photo relates to
    /// </summary>
    public Guid ItemId { get; init; }

    /// <summary>
    /// Title of the photo
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// When the photo was uploaded
    /// </summary>
    public DateTimeOffset Uploaded { get; init; }

    /// <summary>
    /// The photo itself
    /// </summary>
    public required byte[] Data { get; init; }
}
