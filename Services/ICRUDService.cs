using ActualLab.CommandR.Configuration;
using ActualLab.Fusion;
using InventorySharp.Commands;
using InventorySharp.Models;


namespace InventorySharp.Services;

public interface ICRUDService<T> : IComputeService
{
    [ComputeMethod]
    public Task<int> Count(CancellationToken cancellationToken = default);

    [CommandHandler]
    public Task<T> Create(CreateCommand<T> command, CancellationToken cancellationToken = default);

    [ComputeMethod]
    public Task<T> Get(Guid id, CancellationToken cancellationToken = default);

    [ComputeMethod]
    public Task<ICollection<T>> List(CancellationToken cancellationToken = default);

    [CommandHandler]
    public Task Update(UpdateCommand<T> command, CancellationToken cancellationToken = default);

    [CommandHandler]
    public Task Delete(DeleteCommand<T> command,
        CancellationToken cancellationToken = default);
}
