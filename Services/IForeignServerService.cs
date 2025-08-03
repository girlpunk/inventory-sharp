using InventorySharp.Models;

namespace InventorySharp.Services;

/// <inheritdoc />
public interface IForeignServerService : ICRUDService<ForeignServer>
{
    /// <summary>
    /// Find a foreign server by its domain
    /// </summary>
    Task<Guid?> Find(string domain, CancellationToken cancellationToken = default);
}
