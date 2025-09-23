namespace BlazorInventory.Abstractions.Models;

/// <inheritdoc />
/// <summary>
/// Scan of a label
/// </summary>
public class LabelScan : BaseModel
{
    /// <summary>
    /// ID of the label being scanned
    /// </summary>
    public Guid LabelId { get; set; }

    /// <summary>
    /// ID of the label being scanned
    /// </summary>
    public virtual ItemLabel Label { get; set; }

    /// <summary>
    /// ID of the scanner that was used
    /// </summary>
    public Guid? ScannerId { get; set; }

    public virtual Scanner? Scanner { get; set; }

    /// <summary>
    /// Latitude of the scan
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Longitude of the scan
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// Time the scan occurred
    /// </summary>
    public DateTime Scanned { get; set; }

    /// <summary>
    /// Type of scan
    /// </summary>
    public LabelType ScanType { get; set; }
}
