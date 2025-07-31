namespace InventorySharp.Models;

public class ItemTag
{
    public Guid ItemId { get; set; }
    public string Tag { get; set; }

    public virtual Item Item { get; init; }
}
