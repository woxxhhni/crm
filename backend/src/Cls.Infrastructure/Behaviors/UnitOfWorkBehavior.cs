
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Cls.Infrastructure.Persistence;

namespace Cls.Infrastructure.Behaviors
{
    /// <summary>
    /// Ensures one-transaction-per-command. Queries are bypassed by name convention (*Query).
    /// </summary>
    public class UnitOfWorkBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        private readonly AppDbContext _db;

        public UnitOfWorkBehavior(AppDbContext db) => _db = db;

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
        {
            var isQuery = request?.GetType().Name.EndsWith("Query") == true;
            if (isQuery)
                return await next();

            // Use EF execution strategy (retries) + explicit transaction
            var strategy = _db.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _db.Database.BeginTransactionAsync(ct);
                var response = await next();
                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return response;
            });
        }
    }
}
