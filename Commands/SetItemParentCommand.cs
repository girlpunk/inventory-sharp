using System.Reactive;
using System.Text.Json.Serialization;
using ActualLab.CommandR;

namespace InventorySharp.Commands;

[JsonSerializable(typeof(SetItemParentCommand))]
public record SetItemParentCommand : ICommand<Unit>
{
    public Guid ItemId { get; set; }
    public Guid ParentId { get; set; }
}
