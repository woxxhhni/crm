using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cls.Tests.Infrastructure;

public sealed class ClsWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.UseSetting("ConnectionStrings:Default",
            "Host=localhost;Port=5432;Database=Cls_Db;Username=postgres;Password=postgres");
        builder.UseSetting("Jwt:Issuer", "Cls");
        builder.UseSetting("Jwt:Audience", "ClsAudience");
        builder.UseSetting("Jwt:Key", "buM7Iy3wpDeRbLBGqKY3YA8Bla7cbMXZ1VXVu8umxzA=");
        builder.UseSetting("ObjectStorage:Provider", "minio");
        builder.UseSetting("ObjectStorage:Bucket", "cls-temp-bucket");
        builder.UseSetting("Minio:Endpoint", "localhost:9000");
        builder.UseSetting("Minio:PublicEndpoint", "localhost:9000");
        builder.UseSetting("Minio:UseSSL", "false");
        builder.UseSetting("Minio:AccessKey", "clsadmin");
        builder.UseSetting("Minio:SecretKey", "sHKZTjOdoFCCmthE2dsdZg");
        builder.UseSetting("Recaptcha:Enabled", "false");
        builder.UseSetting("Recaptcha:SecretKey", "test");
        builder.UseSetting("Database:MigrateOnStartup", "true");

        builder.ConfigureServices(services =>
        {
            var hosted = services.Where(d => d.ServiceType == typeof(IHostedService)).ToList();
            foreach (var descriptor in hosted)
                services.Remove(descriptor);
        });
    }
}
