using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Cls.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task MigrateAsync(IServiceProvider services, CancellationToken ct = default)
    {
        var cfg = services.GetRequiredService<IConfiguration>();
        var migrate = cfg.GetValue("Database:MigrateOnStartup", true);
        if (!migrate) return;

        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

        // Retry: Postgres may not be ready on first container start
        const int maxAttempts = 10;
        var delay = TimeSpan.FromSeconds(3);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                using var scope = services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // This will CREATE the DB (if missing) and apply ALL pending migrations
                await db.Database.MigrateAsync(ct);

                logger.LogInformation("Database migrated successfully.");

                // Seed default users for all 3 roles
                await SeedUsersAsync(db, logger, ct);

                return;
            }
            catch (NpgsqlException ex) when (attempt < maxAttempts)
            {
                logger.LogWarning(ex, "PostgreSQL not ready (attempt {Attempt}/{Max}). Retrying in {Delay}s…",
                    attempt, maxAttempts, delay.TotalSeconds);
                await Task.Delay(delay, ct);
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                logger.LogWarning(ex, "Migration attempt {Attempt}/{Max} failed. Retrying in {Delay}s…",
                    attempt, maxAttempts, delay.TotalSeconds);
                await Task.Delay(delay, ct);
            }
        }

        // One last try without swallowing exceptions so container crashes if it truly can't migrate
        using (var scope = services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync(ct);
        }
    }

    private static async Task SeedUsersAsync(AppDbContext db, ILogger logger, CancellationToken ct)
    {
        try
        {
            // Check if seed users already exist
            var existingCount = await db.Database
                .SqlQueryRaw<int>("SELECT COUNT(*)::int AS \"Value\" FROM users WHERE email IN ('admin@cls.local', 'manager@cls.local', 'employee@cls.local')")
                .FirstOrDefaultAsync(ct);

            if (existingCount >= 3)
            {
                logger.LogInformation("Seed users already exist. Skipping.");
                return;
            }

            var adminPwd   = BCrypt.Net.BCrypt.HashPassword("Admin@2026!");
            var managerPwd = BCrypt.Net.BCrypt.HashPassword("Manager@2026!");
            var employeePwd = BCrypt.Net.BCrypt.HashPassword("Employee@2026!");

            var sql = $@"
                INSERT INTO users
                (name, email, password_hash, phone, address, description, role,
                 is_active, last_login_at, file_id,
                 created_at, created_by_user_id,
                 updated_at, updated_by_user_id,
                 is_deleted, deleted_at, deleted_by_user_id)
                VALUES
                ('Admin User', 'admin@cls.local', '{adminPwd}', '+1-555-0001', NULL, 'System administrator with full access', 'Admin', TRUE, NULL, NULL, NOW(), 1, NOW(), 1, FALSE, NULL, NULL),
                ('Manager User', 'manager@cls.local', '{managerPwd}', '+1-555-0002', NULL, 'Manager with operational access', 'Manager', TRUE, NULL, NULL, NOW(), 1, NOW(), 1, FALSE, NULL, NULL),
                ('Employee User', 'employee@cls.local', '{employeePwd}', '+1-555-0003', NULL, 'Employee with restricted access', 'Employee', TRUE, NULL, NULL, NOW(), 1, NOW(), 1, FALSE, NULL, NULL)
                ON CONFLICT DO NOTHING;
            ";

            await db.Database.ExecuteSqlRawAsync(sql, ct);
            logger.LogInformation("Seed users created: admin@cls.local, manager@cls.local, employee@cls.local");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to seed users (non-fatal). Users may already exist.");
        }
    }
}

