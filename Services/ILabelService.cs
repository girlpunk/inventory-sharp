using ActualLab.CommandR.Configuration;
using ActualLab.Fusion;
using InventorySharp.Commands;
using InventorySharp.Models;

namespace InventorySharp.Services;

public interface ILabelService : ICRUDService<ItemLabel>
{
    [CommandHandler]
    public Task<ScanLabelResult> Scan(ScanLabelCommand command, CancellationToken cancellationToken = default);

    [ComputeMethod]
    public Task<ICollection<ItemLabel>> List(Guid itemId, CancellationToken cancellationToken = default);
}
