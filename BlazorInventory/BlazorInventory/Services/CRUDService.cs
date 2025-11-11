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

/// <inheritdoc cref="ICRUDService{T}" />
/// <summary>
/// Basic implementation of CRUD operations for a db-backed object
/// </summary>
/// <typeparam name="TModel">Object type</typeparam>
/// <typeparam name="TViewModel">Object type</typeparam>
public abstract class CRUDService<TModel, TViewModel>(IServiceProvider services) : DbServiceBase<ApplicationDbContext>(services), ICRUDService<TViewModel> where TModel : BaseModel where TViewModel : BaseViewModel
{
    /// <inheritdoc />
    [ComputeMethod]
    public virtual async Task<int> Count(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await DbHub.CreateDbContext(cancellationToken);
        return await DbSet(dbContext).CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    [ComputeMethod]
    public virtual async Task<ICollection<TViewModel>> List(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await DbHub.CreateDbContext(cancellationToken);
        return await DbSet(dbContext).ProjectToType<TViewModel>().ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    [ComputeMethod]
    public virtual async Task<TViewModel> Get(Guid id, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await DbHub.CreateDbContext(cancellationToken);
        return await DbSet(dbContext).Where(i => i.Id == id).ProjectToType<TViewModel>().SingleAsync(cancellationToken);
    }

    /// <summary>
    /// How to access the database table
    /// </summary>
    public abstract Func<ApplicationDbContext, DbSet<TModel>> DbSet { get; }

    /// <summary>
    /// Update a database object
    /// </summary>
    /// <param name="input">input values to update from</param>
    /// <param name="output">object to be updated</param>
    public abstract void DoUpdate(TViewModel input, TModel output);

    /// <inheritdoc />
    public virtual async Task Update(UpdateCommand<TViewModel> command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(command.Obj);

        if (Invalidation.IsActive)
        {
            _ = Get(command.Obj.Id, CancellationToken.None);
            _ = List(CancellationToken.None);
            return;
        }

        await using var dbContext = await DbHub.CreateOperationDbContext(cancellationToken);

        var item = await DbSet(dbContext).FindAsync(DbKey.Compose(command.Obj.Id), cancellationToken);

        if (item != null)
            DoUpdate(command.Obj, item);
        else
            throw new InvalidOperationException("Could not find object to update");

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<TViewModel> Create(CreateCommand<TViewModel> command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var context = CommandContext.GetCurrent();
        DbOperationScope<ApplicationDbContext>.GetOrCreate(context).Require();

        if (Invalidation.IsActive)
        {
            var id = context.Operation.Items.KeylessGet<Guid>();

            _ = Get(command.Obj.Id, CancellationToken.None);
            _ = Get(id, CancellationToken.None);
            _ = List(CancellationToken.None);
            _ = Count(CancellationToken.None);

            return null!;
        }

        await using var dbContext = await DbHub.CreateOperationDbContext(cancellationToken);

        var item = (TModel) typeof(TModel).CreateInstance();

        DoUpdate(command.Obj, item);

        DbSet(dbContext).Add(item);
        await dbContext.SaveChangesAsync(cancellationToken);

        context.Operation.Items.KeylessSet(command.Obj.Id);
        return command.Obj;
    }

    /// <inheritdoc />
    public virtual async Task Delete(DeleteCommand<TViewModel> command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var context = CommandContext.GetCurrent();
        DbOperationScope<ApplicationDbContext>.GetOrCreate(context).Require();

        if (Invalidation.IsActive)
        {
            _ = Get(command.Obj.Id, CancellationToken.None);
            _ = List(CancellationToken.None);
            _ = Count(CancellationToken.None);
            return;
        }

        await using var dbContext = await DbHub.CreateOperationDbContext(cancellationToken);

        var item = await DbSet(dbContext).FindAsync(DbKey.Compose(command.Obj.Id), cancellationToken);

        if (item == null)
            throw new InvalidOperationException("Could not find object to delete");

        DbSet(dbContext).Remove(item);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
