using ActualLab.CommandR.Configuration;
using InventorySharp.Commands;
using InventorySharp.Models;

namespace InventorySharp.Services;

public interface IScannerService : ICRUDService<Scanner>
{
    [CommandHandler]
    public Task Scan(AddScanCommand command, CancellationToken cancellationToken = default);
}
