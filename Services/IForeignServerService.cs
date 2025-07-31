using InventorySharp.Models;

namespace InventorySharp.Services;

public interface IForeignServerService : ICRUDService<ForeignServer>
{
    Task<Guid?> Find(string domain, CancellationToken cancellationToken = default);
}
