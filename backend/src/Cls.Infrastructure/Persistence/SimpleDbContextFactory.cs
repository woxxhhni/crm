using Microsoft.EntityFrameworkCore;

namespace Cls.Infrastructure.Persistence;

/// <summary>
/// Simple IDbContextFactory implementation for background services.
/// Creates a new AppDbContext instance with its own DbContextOptions,
/// avoiding the scoped service conflict with the main DI container.
/// </summary>
public class SimpleDbContextFactory : IDbContextFactory<AppDbContext>
{
    private readonly DbContextOptions<AppDbContext> _options;

    public SimpleDbContextFactory(DbContextOptions<AppDbContext> options)
    {
        _options = options;
    }

    public AppDbContext CreateDbContext()
    {
        return new AppDbContext(_options);
    }
}
