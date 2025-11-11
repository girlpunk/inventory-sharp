using ActualLab.Fusion;
using BlazorInventory.Abstractions.ViewModels;

namespace BlazorInventory.Abstractions.Service;

/// <inheritdoc />
public interface IForeignServerService : ICRUDService<ForeignServerView>
{
    /// <summary>
    /// Find a foreign server by its domain
    /// </summary>
    [ComputeMethod]
    Task<Guid?> Find(string domain, CancellationToken cancellationToken = default);
}
