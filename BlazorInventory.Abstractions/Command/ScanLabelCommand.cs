using System.Text.Json.Serialization;
using ActualLab.CommandR;
using BlazorInventory.Abstractions.Models;
using BlazorInventory.Abstractions.Response;

namespace BlazorInventory.Abstractions.Command;

/// <summary>
/// Scan a label, record details (if appropriate) and return information about the label, item, and scan
/// </summary>
[JsonSerializable(typeof(ScanLabelCommand))]
public sealed record ScanLabelCommand : ICommand<ScanLabelResult>
{
    /// <summary>
    /// Identifier that was scanned, if a lookup is required
    /// </summary>
    public string? Identifier { get; set; }

    /// <summary>
    /// Label that was scanned, if lookup already complete
    /// </summary>
    public Guid? LabelId { get; set; }

    /// <summary>
    /// Type of label that was scanned
    /// </summary>
    public LabelType LabelType { get; set; }

    /// <summary>
    /// If a record of this scan should be created
    /// </summary>
    public bool CreateScanRecord { get; set; } = true;

    /// <summary>
    /// Scanner used to make the scan
    /// </summary>
    public Guid? ScannerId { get; set; }

    /// <summary>
    /// Known latitude of the scanner
    /// </summary>
    public double? ScannerLatitude { get; set; }

    /// <summary>
    /// Known longitude of the scanner
    /// </summary>
    public double? ScannerLongitude { get; set; }
}
