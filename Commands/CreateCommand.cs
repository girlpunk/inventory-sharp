using System.Text.Json.Serialization;
using ActualLab.CommandR;

namespace InventorySharp.Commands;

[JsonSerializable(typeof(CreateCommand<>))]
public class CreateCommand<T> : ICommand<T>
{
    public T Obj { get; init; }
}