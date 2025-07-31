using System.Text.Json.Serialization;
using ActualLab.CommandR;
using InventorySharp.Models;

namespace InventorySharp.Commands;

[JsonSerializable(typeof(ScanLabelCommand))]
public record ScanLabelCommand : ICommand<ScanLabelResult>
{
    public string? Identifier { get; set; }
    public Guid? LabelId { get; set; }
    public LabelType LabelType { get; set; }
    public bool CreateScanRecord { get; set; }
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
