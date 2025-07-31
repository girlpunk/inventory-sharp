using ActualLab.Fusion.EntityFramework.Operations;
using Microsoft.EntityFrameworkCore;

namespace InventorySharp.Models;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<ForeignServer> ForeignServers { get; protected set; } = null!;
    public DbSet<Item> Items { get; protected set; } = null!;
    public DbSet<ItemLabel> ItemLabels { get; protected set; } = null!;
    public DbSet<ItemPhoto> ItemPhotos { get; protected set; } = null!;
    public DbSet<LabelScan> LabelScans { get; protected set; } = null!;
    public DbSet<ItemTag> ItemTags { get; protected set; } = null!;
    public DbSet<Scanner> Scanners { get; protected set; } = null!;

    // ActualLab.Fusion.EntityFramework.Operations tables
    public DbSet<DbOperation> Operations { get; protected set; } = null!;
    public DbSet<DbEvent> Events { get; protected set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
