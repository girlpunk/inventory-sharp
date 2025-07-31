using System.ComponentModel.DataAnnotations;

namespace InventorySharp.Models;

/// <summary>
/// Label scanner
/// </summary>
public class Scanner : BaseModel
{
    /// <summary>
    /// Name of the scanner
    /// </summary>
    [MaxLength(128)]
    public string Name { get; set; }

    /// <summary>
    /// Types of label that can be scanned
    /// </summary>
    public LabelType ScannerType { get; set; }

    /// <summary>
    /// Known latitude of the scanner
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Known longitude of the scanner
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// Parent item associated when an item is scanned
    /// </summary>
    public Guid? ParentItemId { get; set; }
}
