using ActualLab.CommandR.Configuration;
using ActualLab.Fusion;
using BlazorInventory.Abstractions.Command;

namespace BlazorInventory.Abstractions.Service;

/// <summary>
/// Standard Create/Read/Update/Delete operations
/// </summary>
/// <typeparam name="TViewModel"/>
public interface ICRUDService<TViewModel> : IComputeService
{
    /// <summary>
    /// Count of items
    /// </summary>
    [ComputeMethod]
    public Task<int> Count(CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new item
    /// </summary>
    [CommandHandler]
    public Task<TViewModel> Create(CreateCommand<TViewModel> command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get an existing item
    /// </summary>
    [ComputeMethod]
    public Task<TViewModel> Get(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// List all items
    /// </summary>
    [ComputeMethod]
    public Task<ICollection<TViewModel>> List(CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing item
    /// </summary>
    [CommandHandler]
    public Task Update(UpdateCommand<TViewModel> command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete an item
    /// </summary>
    [CommandHandler]
    public Task Delete(DeleteCommand<TViewModel> command,
        CancellationToken cancellationToken = default);
}
