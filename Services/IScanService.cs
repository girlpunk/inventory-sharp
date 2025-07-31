using ActualLab.Fusion;
using InventorySharp.Models;

namespace InventorySharp.Services;

public interface IScanService : ICRUDService<LabelScan>
{
    [ComputeMethod]
    public Task<ICollection<LabelScan>> List(Guid itemId, CancellationToken cancellationToken = default);
}
