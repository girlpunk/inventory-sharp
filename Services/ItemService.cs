using InventorySharp.Models;
using Microsoft.EntityFrameworkCore;

namespace InventorySharp.Services;

/// <inheritdoc cref="IItemService"  />
public class ItemService(IServiceProvider serviceProvider) : CRUDService<Item>(serviceProvider), IItemService
{
    /// <inheritdoc />
    public override Func<AppDbContext, DbSet<Item>> DbSet => static context => context.Items;

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
}