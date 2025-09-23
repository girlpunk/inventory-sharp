using ActualLab.Fusion;
using BlazorInventory.Abstractions.Models;

namespace BlazorInventory.Abstractions.Service;

/// <inheritdoc />
public interface IPhotoService : ICRUDService<ItemPhoto>
{
    /// <summary>
    /// List photos for a specific item
    /// </summary>
    [ComputeMethod]
    public Task<ICollection<ItemPhoto>> List(Guid itemId, CancellationToken cancellationToken = default);
}
