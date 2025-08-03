using System.Text.Json.Serialization;
using ActualLab.CommandR;

namespace InventorySharp.Commands;

/// <summary>
/// Create a new <c>T</c>
/// </summary>
/// <typeparam name="T">Type of object to be created</typeparam>
[JsonSerializable(typeof(CreateCommand<>))]
public record CreateCommand<T> : ICommand<T>
{
    /// <summary>
    /// Object to be created
    /// </summary>
    public required T Obj { get; init; }
}
