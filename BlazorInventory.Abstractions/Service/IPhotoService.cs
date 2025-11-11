using ActualLab.Fusion;
using BlazorInventory.Abstractions.ViewModels;

namespace BlazorInventory.Abstractions.Service;

/// <inheritdoc />
public interface IPhotoService : ICRUDService<ItemPhotoView>
{
    /// <summary>
    /// List photos for a specific item
    /// </summary>
    [ComputeMethod]
    public Task<ICollection<ItemPhotoView>> List(Guid itemId, CancellationToken cancellationToken = default);
}
