using System.Reactive;
using System.Text.Json.Serialization;
using ActualLab.CommandR;

namespace InventorySharp.Commands;

/// <summary>
/// Set the parent of an item
/// </summary>
[JsonSerializable(typeof(SetItemParentCommand))]
public record SetItemParentCommand : ICommand<Unit>
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
