using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySharp.Models;

/// <summary>
/// Shared base for models, so we can do CRUD easily
/// </summary>
public abstract class BaseModel
{
    /// <summary>
    /// Unique Identifier
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid Id { get; set; }
}
