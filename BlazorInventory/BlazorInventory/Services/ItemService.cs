using BlazorInventory.Abstractions.Service;
using BlazorInventory.Data;
using Microsoft.EntityFrameworkCore;
using ActualLab.Fusion;
using BlazorInventory.Abstractions.ViewModels;
using BlazorInventory.Data.Models;
using Mapster;

namespace BlazorInventory.Services;

/// <inheritdoc cref="IItemService"  />
public class ItemService(IServiceProvider serviceProvider) : CRUDService<Item, ItemView>(serviceProvider), IItemService
{
    /// <inheritdoc />
    public override Func<ApplicationDbContext, DbSet<Item>> DbSet => static context => context.Items;

    /// <inheritdoc />
    public override void DoUpdate(ItemView input, Item output)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(input);

        output.Created = input.Created;
        output.Description = input.Description;
        output.Name = input.Name;
        output.ParentId = input.ParentId;

        if (input.Id != null)
            output.Id = input.Id.Value;
    }

    /// <inheritdoc />
    [ComputeMethod]
    public virtual async Task<ICollection<ItemView>> ListChildren(Guid id, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await DbHub.CreateDbContext(cancellationToken);
        return await dbContext.Items.Where(i => i.ParentId == id).ProjectToType<ItemView>().ToListAsync(cancellationToken);
    }
}
