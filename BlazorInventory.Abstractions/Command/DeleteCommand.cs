using System.Text.Json.Serialization;
using ActualLab.CommandR;

namespace BlazorInventory.Abstractions.Command;

/// <summary>
/// Delete an instance of <c>T</c>
/// </summary>
/// <typeparam name="T">Type of object to be deleted</typeparam>
[JsonSerializable(typeof(CreateCommand<>))]
public sealed record DeleteCommand<T> : ICommand<T>
{
    /// <summary>
    /// Object to be deleted
    /// </summary>
    public required T Obj { get; init; }
}
