using System.Reactive;
using ActualLab.CommandR;
using MemoryPack;

namespace BlazorInventory.Abstractions.Command;

/// <summary>
/// Set the parent of an item
/// </summary>
[MemoryPackable]
public sealed partial record SetItemParentCommand : ICommand<Unit>
{
    /// <summary>
    /// Item to be updated
    /// </summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// Item to set as parent
    /// </summary>
    public Guid ParentId { get; set; }
}
