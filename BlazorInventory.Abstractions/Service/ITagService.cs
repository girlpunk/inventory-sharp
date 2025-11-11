using ActualLab.Fusion;
using BlazorInventory.Abstractions.ViewModels;

namespace BlazorInventory.Abstractions.Service;

/// <inheritdoc />
public interface ITagService : ICRUDService<ItemTagView>
{
    /// <summary>
    /// List tags for a given item
    /// </summary>
    [ComputeMethod]
    public Task<ICollection<ItemTagView>> List(Guid itemId, CancellationToken cancellationToken = default);
}
