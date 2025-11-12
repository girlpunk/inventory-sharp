using ActualLab.CommandR;
using MemoryPack;

namespace BlazorInventory.Abstractions.Command;

/// <summary>
/// Delete an instance of <c>T</c>
/// </summary>
/// <typeparam name="T">Type of object to be deleted</typeparam>
[MemoryPackable]
public sealed partial record DeleteCommand<T> : ICommand<T>
{
    /// <summary>
    /// Object to be deleted
    /// </summary>
    public required T Obj { get; init; }
}
