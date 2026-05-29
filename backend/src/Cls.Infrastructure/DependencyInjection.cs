using Cls.Application.Abstractions;
using Cls.Infrastructure.Auth;
using Cls.Infrastructure.Background;
using Cls.Infrastructure.Persistence;
using Cls.Infrastructure.Persistence.Repository;
using Cls.Infrastructure.Serialization;
using Cls.Infrastructure.Storage;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cls.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IHostApplicationBuilder applicationBuilder)
    {
        var services = applicationBuilder.Services;

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Behaviors.UnitOfWorkBehavior<,>));

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var cfg = sp.GetRequiredService<IConfiguration>();
            var env = sp.GetRequiredService<IHostEnvironment>();

            var provider = (cfg["Database:Provider"] ?? "postgres")
                           .Trim().ToLowerInvariant();

            var conn = cfg.GetConnectionString("Default")
                       ?? throw new InvalidOperationException("ConnectionStrings:Default is missing.");

            switch (provider)
            {
                case "postgres":
                case "postgresql":
                    options.UseNpgsql(conn);
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported Database:Provider '{provider}'. Use 'postgres'.");
            }
        });

        // DbContextFactory for background services (uses its own options, avoids scope conflict)
        services.AddSingleton<IDbContextFactory<AppDbContext>>(sp =>
        {
            var cfg = sp.GetRequiredService<IConfiguration>();
            var conn = cfg.GetConnectionString("Default")
                       ?? throw new InvalidOperationException("ConnectionStrings:Default is missing.");

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(conn);
            var options = optionsBuilder.Options;

            return new SimpleDbContextFactory(options);
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Object Storage
        services.AddSingleton<IObjectStorageService>(sp =>
        {
            var cfg = sp.GetRequiredService<IConfiguration>();
            var provider = (cfg["ObjectStorage:Provider"] ?? "minio").Trim().ToLowerInvariant();

            return provider switch
            {
                "minio" => new MinioObjectStorageService(cfg),
                _ => throw new InvalidOperationException(
                    $"Unsupported ObjectStorage:Provider '{provider}'. Use 'minio'.")
            };
        });

        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IJsonSerializer, SystemTextJsonSerializer>();
        services.AddScoped<IRecaptchaService, RecaptchaService>();
        services.AddHttpClient();

        // Background services
        services.AddHostedService<BackupExportService>();
        services.AddHostedService<BackupRestoreService>();

        return services;
    }
}

