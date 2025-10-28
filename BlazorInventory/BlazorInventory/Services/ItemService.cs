using BlazorInventory.Abstractions.Models;
using BlazorInventory.Abstractions.Service;
using BlazorInventory.Data;
using Microsoft.EntityFrameworkCore;
using ActualLab.Fusion;

namespace BlazorInventory.Services;

/// <inheritdoc cref="IItemService"  />
public class ItemService(IServiceProvider serviceProvider) : CRUDService<Item>(serviceProvider), IItemService
{
    /// <inheritdoc />
    public override Func<ApplicationDbContext, DbSet<Item>> DbSet => static context => context.Items;

    /// <inheritdoc />
    public override void DoUpdate(Item input, Item output)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(input);

        output.Created = input.Created;
        output.Description = input.Description;
        output.Name = input.Name;
        output.ParentId = input.ParentId;
        output.Id = input.Id;
    }

    /// <inheritdoc />
    [ComputeMethod]
    public virtual async Task<ICollection<Item>> ListChildren(Guid id, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await DbHub.CreateDbContext(cancellationToken);
        return await dbContext.Items.Where(i => i.ParentId == id).ToListAsync(cancellationToken).ConfigureAwait(false);
    }
}
