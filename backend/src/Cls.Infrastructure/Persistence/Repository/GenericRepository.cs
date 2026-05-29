using Microsoft.EntityFrameworkCore;
using Cls.Application.Abstractions;
using Cls.Domain.Common;
using Cls.Shared.Contracts.Abstractions;
using Cls.Shared.Data;
using Cls.Shared.Exceptions;

namespace Cls.Infrastructure.Persistence.Repository;
public class GenericRepository<T>(AppDbContext ctx, ICurrentUserService currentUserService) : IGenericRepository<T> where T : BaseEntity
{
    public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default, bool includeDeleted = false)
    {
        var q = includeDeleted ?
                ctx.Set<T>().IgnoreQueryFilters().AsQueryable() :
                ctx.Set<T>().AsQueryable();
        return await q.FirstOrDefaultAsync(p => p.Id == id, ct) ?? throw new NotFoundException();
    }
    public async Task<IReadOnlyList<T>> ListAsync(System.Linq.Expressions.Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default, bool includeDeleted = false)
    {
        var q = includeDeleted ?
                ctx.Set<T>().IgnoreQueryFilters().AsQueryable() :
                ctx.Set<T>().AsQueryable();
        if (predicate != null)
            q = q.Where(predicate);
        return await q.ToListAsync(ct);
    }

    public async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.CreatedByUserId = currentUserService.UserId;
        entity.UpdatedByUserId = currentUserService.UserId;

        await ctx.Set<T>().AddAsync(entity, ct);
        return entity;
    }
    public Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedByUserId = currentUserService.UserId;
        ctx.Set<T>().Update(entity);
        return Task.CompletedTask;
    }
    public Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        if (entity is ISoftDeletable soft)
        {
            soft.IsDeleted = true;
            soft.DeletedAt = DateTime.UtcNow;
            soft.DeletedByUserId = currentUserService.UserId;

            ctx.Set<T>().Update(entity);
        }
        else
        {
            ctx.Set<T>().Remove(entity);
        }

        return Task.CompletedTask;
    }
    public Task HardDeleteAsync(T entity, CancellationToken ct = default)
    {
        ctx.Set<T>().Remove(entity);
        return Task.CompletedTask;
    }
    public IQueryable<T> Query(bool includeDeleted = false) => includeDeleted ? ctx.Set<T>().IgnoreQueryFilters().AsQueryable() : ctx.Set<T>().AsQueryable();
}
