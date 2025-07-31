using System.Reactive;
using System.Text.Json.Serialization;
using ActualLab.CommandR;

namespace InventorySharp.Commands;

[JsonSerializable(typeof(AddScanCommand))]
public record AddScanCommand : ICommand<Unit>
{

}