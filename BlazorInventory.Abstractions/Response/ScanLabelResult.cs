using BlazorInventory.Abstractions.ViewModels;
using MemoryPack;

namespace BlazorInventory.Abstractions.Response;

/// <summary>
/// Results of completing a scan
/// </summary>
[MemoryPackable]
public sealed partial record ScanLabelResult
{
    /// <summary>
    /// Label that was scanned
    /// </summary>
    public ItemLabelView? Label { get; set; }

    /// <summary>
    /// Record of the scan
    /// </summary>
    public LabelScanView? Scan { get; set; }

    /// <summary>
    /// Item that was scanned
    /// </summary>
    public ItemView? Item { get; set; }
}
