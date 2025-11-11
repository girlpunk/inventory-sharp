using ActualLab;
using ActualLab.Collections;
using ActualLab.CommandR;
using ActualLab.Fusion;
using ActualLab.Fusion.EntityFramework;
using ActualLab.Fusion.EntityFramework.Operations;
using ActualLab.Reflection;
using BlazorInventory.Abstractions.Command;
using BlazorInventory.Abstractions.Service;
using BlazorInventory.Abstractions.ViewModels;
using BlazorInventory.Data;
using BlazorInventory.Data.Models;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace BlazorInventory.Services;

public class TagService(IServiceProvider serviceProvider) : DbServiceBase<ApplicationDbContext>(serviceProvider), ITagService
{
    /// <inheritdoc />
    [ComputeMethod]
    public virtual async Task<ICollection<ItemTagView>> List(Guid itemId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await DbHub.CreateDbContext(cancellationToken);
        return await dbContext.ItemTags.Where(l => l.ItemId == itemId).ProjectToType<ItemTagView>().ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    [ComputeMethod]
    public virtual async Task<int> Count(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await DbHub.CreateDbContext(cancellationToken);
        return await dbContext.ItemTags.CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    [ComputeMethod]
    public virtual async Task<ICollection<ItemTagView>> List(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await DbHub.CreateDbContext(cancellationToken);
        return await dbContext.ItemTags.ProjectToType<ItemTagView>().ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    [ComputeMethod]
    public virtual async Task<ItemTagView> Get(Guid id, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await DbHub.CreateDbContext(cancellationToken);
        return await dbContext.ItemTags.Where(i => i.ItemId == id).ProjectToType<ItemTagView>().SingleAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task Update(UpdateCommand<ItemTagView> command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(command.Obj);

        if (Invalidation.IsActive)
        {
            _ = Get(command.Obj.ItemId, CancellationToken.None);
            _ = List(CancellationToken.None);
            return;
        }

        await using var dbContext = await DbHub.CreateOperationDbContext(cancellationToken);

        var item = await dbContext.ItemTags.FindAsync(DbKey.Compose(command.Obj.ItemId, command.Obj.Tag), cancellationToken);

        if (item != null)
        {
            item.ItemId = command.Obj.ItemId;
            item.Tag = command.Obj.Tag;
        }
        else
        {
            throw new InvalidOperationException("Could not find object to update");
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<ItemTagView> Create(CreateCommand<ItemTagView> command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var context = CommandContext.GetCurrent();
        DbOperationScope<ApplicationDbContext>.GetOrCreate(context).Require();

        if (Invalidation.IsActive)
        {
            var itemId = context.Operation.Items.Get<Guid>("itemId");

            _ = Get(command.Obj.ItemId, CancellationToken.None);
            _ = Get(itemId, CancellationToken.None);
            _ = List(CancellationToken.None);
            _ = Count(CancellationToken.None);

            return null!;
        }

        await using var dbContext = await DbHub.CreateOperationDbContext(cancellationToken);

        var item = (ItemTag) typeof(ItemTag).CreateInstance();

        item.ItemId = command.Obj.ItemId;
        item.Tag = command.Obj.Tag;

        dbContext.ItemTags.Add(item);
        await dbContext.SaveChangesAsync(cancellationToken);

        context.Operation.Items.Set("itemId", command.Obj.ItemId);
        return command.Obj;
    }

    /// <inheritdoc />
    public virtual async Task Delete(DeleteCommand<ItemTagView> command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var context = CommandContext.GetCurrent();
        DbOperationScope<ApplicationDbContext>.GetOrCreate(context).Require();

        if (Invalidation.IsActive)
        {
            _ = Get(command.Obj.ItemId, CancellationToken.None);
            _ = List(CancellationToken.None);
            _ = Count(CancellationToken.None);
            return;
        }

        await using var dbContext = await DbHub.CreateOperationDbContext(cancellationToken);

        var item = await dbContext.ItemTags.FindAsync(DbKey.Compose(command.Obj.ItemId, command.Obj.Tag), cancellationToken);

        if (item == null)
            throw new InvalidOperationException("Could not find object to delete");

        dbContext.ItemTags.Remove(item);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}