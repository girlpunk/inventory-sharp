using ActualLab.Fusion;
using BlazorInventory.Abstractions.Models;

namespace BlazorInventory.Abstractions.Service;

/// <inheritdoc />
public interface IForeignServerService : ICRUDService<ForeignServer>
{
    /// <summary>
    /// Find a foreign server by its domain
    /// </summary>
    [ComputeMethod]
    Task<Guid?> Find(string domain, CancellationToken cancellationToken = default);
}
