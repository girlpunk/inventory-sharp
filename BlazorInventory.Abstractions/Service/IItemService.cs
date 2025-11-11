using ActualLab.Fusion;
using BlazorInventory.Abstractions.ViewModels;

namespace BlazorInventory.Abstractions.Service;

/// <inheritdoc />
public interface IItemService : ICRUDService<ItemView>
{
    /// <summary>
    /// List the direct children of a given item
    /// </summary>
    /// <param name="id">Parent item</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [ComputeMethod]
    public Task<ICollection<ItemView>> ListChildren(Guid id, CancellationToken cancellationToken = default);
}
