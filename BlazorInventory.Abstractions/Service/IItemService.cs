using ActualLab.Fusion;
using BlazorInventory.Abstractions.Models;

namespace BlazorInventory.Abstractions.Service;

/// <inheritdoc />
public interface IItemService : ICRUDService<Item>
{
    /// <summary>
    /// List the direct children of a given item
    /// </summary>
    /// <param name="id">Parent item</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [ComputeMethod]
    public Task<ICollection<Item>> ListChildren(Guid id, CancellationToken cancellationToken = default);
}
