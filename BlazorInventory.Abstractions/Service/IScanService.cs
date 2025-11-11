using ActualLab.Fusion;
using BlazorInventory.Abstractions.ViewModels;

namespace BlazorInventory.Abstractions.Service;

/// <inheritdoc />
public interface IScanService : ICRUDService<LabelScanView>
{
    /// <summary>
    /// List scans for a given item
    /// </summary>
    [ComputeMethod]
    public Task<ICollection<LabelScanView>> List(Guid itemId, CancellationToken cancellationToken = default);
}
