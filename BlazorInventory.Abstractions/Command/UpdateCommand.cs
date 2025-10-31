using System.Text.Json.Serialization;
using ActualLab.CommandR;

namespace BlazorInventory.Abstractions.Command;

/// <summary>
/// Update an item
/// </summary>
/// <typeparam name="T">Type of the item</typeparam>
[JsonSerializable(typeof(CreateCommand<>))]
public sealed record UpdateCommand<T> : ICommand<T>
{
    /// <summary>
    /// Item to be updated
    /// </summary>
    public required T Obj { get; init; }
}
