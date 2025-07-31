namespace InventorySharp.Commands;

public record ScanLabelResult
{
    public Guid? LabelId { get; set; }
    public Guid? ScanId { get; set; }
}
