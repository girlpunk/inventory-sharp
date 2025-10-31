using BlazorInventory.Abstractions.Models;

namespace BlazorInventory.Abstractions.Response;

/// <summary>
/// Results of completing a scan
/// </summary>
public sealed record ScanLabelResult
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
