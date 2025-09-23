using ActualLab.Fusion;
using BlazorInventory.Abstractions.Models;

namespace BlazorInventory.Abstractions.Service;

/// <inheritdoc />
public interface IScanService : ICRUDService<LabelScan>
{
    /// <summary>
    /// List scans for a given item
    /// </summary>
    [ComputeMethod]
    public Task<ICollection<LabelScan>> List(Guid itemId, CancellationToken cancellationToken = default);
}
