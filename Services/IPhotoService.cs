using ActualLab.Fusion;
using InventorySharp.Models;

namespace InventorySharp.Services;

public interface IPhotoService : ICRUDService<ItemPhoto>
{
    [ComputeMethod]
    public Task<ICollection<ItemPhoto>> List(Guid itemId, CancellationToken cancellationToken = default);
}
