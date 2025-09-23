using ActualLab.CommandR.Configuration;
using ActualLab.Fusion;
using BlazorInventory.Abstractions.Command;
using BlazorInventory.Abstractions.Models;
using BlazorInventory.Abstractions.Response;

namespace BlazorInventory.Abstractions.Service;

/// <inheritdoc />
public interface ILabelService : ICRUDService<ItemLabel>
{
    /// <summary>
    /// Scan a label
    /// </summary>
    [CommandHandler]
    public Task<ScanLabelResult> Scan(ScanLabelCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// List labels for a given item
    /// </summary>
    [ComputeMethod]
    public Task<ICollection<ItemLabel>> List(Guid itemId, CancellationToken cancellationToken = default);
}
