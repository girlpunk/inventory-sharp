using ActualLab.Fusion;
using InventorySharp.Models;

namespace InventorySharp.Services;

public interface ITagService : ICRUDService<ItemTag>
{
    [ComputeMethod]
    public Task<ICollection<ItemTag>> List(Guid itemId, CancellationToken cancellationToken = default);
}
