using ActualLab.Fusion;
using InventorySharp.Models;

namespace InventorySharp.Services;

/// <inheritdoc />
public interface IPhotoService : ICRUDService<ItemPhoto>
{
    /// <summary>
    /// List photos for a specific item
    /// </summary>
    [ComputeMethod]
    public Task<ICollection<ItemPhoto>> List(Guid itemId, CancellationToken cancellationToken = default);
}
