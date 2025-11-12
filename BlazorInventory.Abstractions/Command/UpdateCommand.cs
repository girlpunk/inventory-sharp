using ActualLab.CommandR;
using MemoryPack;

namespace BlazorInventory.Abstractions.Command;

/// <summary>
/// Update an item
/// </summary>
/// <typeparam name="T">Type of the item</typeparam>
[MemoryPackable]
public sealed partial record UpdateCommand<T> : ICommand<T>
{
    /// <summary>
    /// Item to be updated
    /// </summary>
    public required T Obj { get; init; }
}
