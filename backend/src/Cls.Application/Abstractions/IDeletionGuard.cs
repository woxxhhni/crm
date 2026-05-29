using Cls.Domain.Common;

namespace Cls.Application.Abstractions;

public interface IDeletionGuard<in T> where T : SoftDeletableEntity
{
    Task EnsureCanDeleteAsync(T entity, CancellationToken ct = default);
}
