using System.Reactive;
using System.Text.Json.Serialization;
using ActualLab.CommandR;

namespace BlazorInventory.Abstractions.Command;

/// <summary>
/// Set the parent of an item
/// </summary>
[JsonSerializable(typeof(SetItemParentCommand))]
public sealed record SetItemParentCommand : ICommand<Unit>
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
