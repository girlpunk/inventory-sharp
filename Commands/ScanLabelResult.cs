using InventorySharp.Models;

namespace InventorySharp.Commands;

/// <summary>
/// Results of completing a scan
/// </summary>
public record ScanLabelResult
{
    /// <summary>
    /// Label that was scanned
    /// </summary>
    public ItemLabel? Label { get; set; }

    /// <summary>
    /// Record of the scan
    /// </summary>
    public LabelScan? Scan { get; set; }

    /// <summary>
    /// Item that was scanned
    /// </summary>
    public Item? Item { get; set; }
}
