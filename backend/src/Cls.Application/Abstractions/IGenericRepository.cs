using Cls.Domain.Common;
using System.Linq.Expressions;
namespace Cls.Application.Abstractions;
public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default, bool includeDeleted = false);
    Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default, bool includeDeleted = false);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(T entity, CancellationToken ct = default);
    Task HardDeleteAsync(T entity, CancellationToken ct = default);
    IQueryable<T> Query(bool includeDeleted = false);
}
