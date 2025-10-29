using ActualLab;
using ActualLab.Collections;
using ActualLab.CommandR;
using ActualLab.Fusion;
using ActualLab.Fusion.EntityFramework;
using ActualLab.Fusion.EntityFramework.Operations;
using BlazorInventory.Abstractions.Command;
using BlazorInventory.Abstractions.Models;
using BlazorInventory.Abstractions.Service;
using BlazorInventory.Data;
using Microsoft.EntityFrameworkCore;

namespace BlazorInventory.Services;

/// <inheritdoc cref="ICRUDService" />
/// <summary>
/// Basic implementation of CRUD operations for a db-backed object
/// </summary>
/// <typeparam name="T">Object type</typeparam>
public abstract class CRUDService<T>(IServiceProvider services) : DbServiceBase<ApplicationDbContext>(services), ICRUDService<T> where T : BaseModel
{
    /// <inheritdoc />
    [ComputeMethod]
    public virtual async Task<int> Count(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await DbHub.CreateDbContext(cancellationToken);
        return await DbSet(dbContext).CountAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    [ComputeMethod]
    public virtual async Task<ICollection<T>> List(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await DbHub.CreateDbContext(cancellationToken);
        return await DbSet(dbContext).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    [ComputeMethod]
    public virtual async Task<T> Get(Guid id, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await DbHub.CreateDbContext(cancellationToken);
        return await DbSet(dbContext).SingleAsync(i => i.Id == id, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// How to access the database table
    /// </summary>
    public abstract Func<ApplicationDbContext, DbSet<T>> DbSet { get; }

    /// <summary>
    /// Update a database object
    /// </summary>
    /// <param name="input">input values to update from</param>
    /// <param name="output">object to be updated</param>
    public abstract void DoUpdate(T input, T output);

    /// <inheritdoc />
    public virtual async Task Update(UpdateCommand<T> command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(command.Obj);

        if (Invalidation.IsActive)
        {
            _ = Get(command.Obj.Id, default);
            _ = List(default);
            return;
        }

        await using var dbContext = await DbHub.CreateOperationDbContext(cancellationToken);

        var item = await DbSet(dbContext).FindAsync(DbKey.Compose(command.Obj.Id), cancellationToken).ConfigureAwait(false);

        if (item != null)
            DoUpdate(command.Obj, item);
        else
            DbSet(dbContext).Add(command.Obj);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<T> Create(CreateCommand<T> command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var context = CommandContext.GetCurrent();
        DbOperationScope<ApplicationDbContext>.GetOrCreate(context).Require();

        if (Invalidation.IsActive)
        {
            var id = context.Operation.Items.KeylessGet<Guid>();

            _ = Get(command.Obj.Id, default);
            _ = Get(id, default);
            _ = List(default);
            _ = Count(default);

            return default!;
        }

        await using var dbContext = await DbHub.CreateOperationDbContext(cancellationToken);

        DbSet(dbContext).Add(command.Obj);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        context.Operation.Items.KeylessSet(command.Obj.Id);
        return command.Obj;
    }

    /// <inheritdoc />
    public virtual async Task Delete(DeleteCommand<T> command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var context = CommandContext.GetCurrent();
        DbOperationScope<ApplicationDbContext>.GetOrCreate(context).Require();

        if (Invalidation.IsActive)
        {
            _ = Get(command.Obj.Id, default);
            _ = List(default);
            _ = Count(default);
            return;
        }

        await using var dbContext = await DbHub.CreateOperationDbContext(cancellationToken);

        DbSet(dbContext).Remove(command.Obj);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
