using ActualLab.Fusion.EntityFramework.Operations;
using BlazorInventory.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlazorInventory.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    /// <summary>
    /// Other inventory servers we know about
    /// </summary>
    public DbSet<ForeignServer> ForeignServers { get; protected set; } = null!;

    /// <summary>
    /// Things that are inventoried
    /// </summary>
    public DbSet<Item> Items { get; protected set; } = null!;

    /// <summary>
    /// Labels for Items
    /// </summary>
    public DbSet<ItemLabel> ItemLabels { get; protected set; } = null!;

    /// <summary>
    /// Photos of Items
    /// </summary>
    public DbSet<ItemPhoto> ItemPhotos { get; protected set; } = null!;

    /// <summary>
    /// Scans of Labels
    /// </summary>
    public DbSet<LabelScan> LabelScans { get; protected set; } = null!;

    /// <summary>
    /// Tags for Items
    /// </summary>
    public DbSet<ItemTag> ItemTags { get; protected set; } = null!;

    /// <summary>
    /// Label Scanners
    /// </summary>
    public DbSet<Scanner> Scanners { get; protected set; } = null!;

    // ActualLab.Fusion.EntityFramework.Operations tables
    public DbSet<DbOperation> Operations { get; protected set; } = null!;
    public DbSet<DbEvent> Events { get; protected set; } = null!;

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ItemTag>()
            .HasKey(static e => new { e.ItemId, e.Tag });

        modelBuilder.Entity<ForeignServer>()
            .HasIndex(static f => f.Namespace)
            .IsUnique();

        modelBuilder.Entity<ItemLabel>()
            .HasIndex(static l => new { l.ForeignServerId, l.Identifier })
            .IsUnique();
    }
}
