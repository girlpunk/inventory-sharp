using ActualLab.Fusion;
using BlazorInventory.Abstractions.Models;

namespace BlazorInventory.Abstractions.Service;

/// <inheritdoc />
public interface ITagService : ICRUDService<ItemTag>
{
    /// <summary>
    /// List tags for a given item
    /// </summary>
    [ComputeMethod]
    public Task<ICollection<ItemTag>> List(Guid itemId, CancellationToken cancellationToken = default);
}
