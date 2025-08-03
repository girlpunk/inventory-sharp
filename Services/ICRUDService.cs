using ActualLab.CommandR.Configuration;
using ActualLab.Fusion;
using InventorySharp.Commands;


namespace InventorySharp.Services;

/// <summary>
/// Standard Create/Read/Update/Delete operations
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ICRUDService<T> : IComputeService
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
    public Task<T> Create(CreateCommand<T> command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get an existing item
    /// </summary>
    [ComputeMethod]
    public Task<T> Get(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// List all items
    /// </summary>
    [ComputeMethod]
    public Task<ICollection<T>> List(CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing item
    /// </summary>
    [CommandHandler]
    public Task Update(UpdateCommand<T> command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete an item
    /// </summary>
    [CommandHandler]
    public Task Delete(DeleteCommand<T> command,
        CancellationToken cancellationToken = default);
}
