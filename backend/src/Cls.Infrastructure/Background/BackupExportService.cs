using System.Globalization;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cls.Application.Abstractions;
using Cls.Domain.Enums;
using Cls.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cls.Infrastructure.Background;

/// <summary>
/// Background worker that processes pending backup export jobs.
/// Exports all DB tables as JSON, generates CSV summaries, downloads MinIO assets,
/// and packages everything into a ZIP stored at system/backups/.
/// </summary>
public class BackupExportService : BackgroundService
{
    private readonly ILogger<BackupExportService> _log;
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IObjectStorageService _storage;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public BackupExportService(ILogger<BackupExportService> log, IDbContextFactory<AppDbContext> dbFactory, IObjectStorageService storage)
    {
        _log = log;
        _dbFactory = dbFactory;
        _storage = storage;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log.LogInformation("BackupExportService started.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(stoppingToken);

                var job = await db.BackupJobs
                    .Where(x => x.Status == BackupJobStatus.Pending && x.Type == BackupJobType.Export)
                    .OrderBy(x => x.CreatedAt)
                    .FirstOrDefaultAsync(stoppingToken);

                if (job is not null)
                {
                    await ProcessExportJob(db, job, stoppingToken);
                    await EnforceRetentionLimit(db, stoppingToken);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _log.LogError(ex, "BackupExportService loop error.");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
        _log.LogInformation("BackupExportService stopped.");
    }

    private async Task ProcessExportJob(AppDbContext db, Domain.Entities.BackupJob job, CancellationToken ct)
    {
        job.Status = BackupJobStatus.InProgress;
        job.StartedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        _log.LogInformation("Processing backup export job {JobId}", job.Id);

        try
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var fileName = $"backup_{timestamp}.zip";
            var filePath = $"system/backups/{fileName}";

            // Ensure MinIO bucket exists before any storage operations
            await _storage.EnsureBucketExistsAsync(ct);

            using var zipStream = new MemoryStream();
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                // 1. Export all entity tables as JSON
                await ExportAllTablesToJson(db, archive, ct);

                // 2. Generate CSV summaries
                await ExportCsvSummaries(db, archive, ct);

                // 3. Download and include MinIO assets
                await ExportMinioAssets(archive, ct);

                // 4. Build manifest
                await WriteManifest(db, archive, timestamp, ct);
            }

            // Upload ZIP to MinIO
            zipStream.Position = 0;
            await _storage.UploadAsync(filePath, zipStream, "application/zip", zipStream.Length, ct);

            job.Status = BackupJobStatus.Completed;
            job.FileName = fileName;
            job.FilePath = filePath;
            job.FileSizeBytes = zipStream.Length;
            job.CompletedAt = DateTime.UtcNow;
            _log.LogInformation("Backup export job {JobId} completed. Size: {Size} bytes", job.Id, zipStream.Length);
        }
        catch (Exception ex)
        {
            job.Status = BackupJobStatus.Failed;
            job.ErrorMessage = ex.Message.Length > 4000 ? ex.Message[..4000] : ex.Message;
            job.CompletedAt = DateTime.UtcNow;
            _log.LogError(ex, "Backup export job {JobId} failed.", job.Id);
        }

        await db.SaveChangesAsync(ct);
    }

    private async Task ExportAllTablesToJson(AppDbContext db, ZipArchive archive, CancellationToken ct)
    {
        // Use raw SQL for ALL tables to ensure JSON keys match PostgreSQL column names (snake_case).
        // This guarantees round-trip compatibility with the import process.
        await WriteJsonRaw(db, archive, "data/users.json", "SELECT * FROM users", ct);
        await WriteJsonRaw(db, archive, "data/clients.json", "SELECT * FROM clients", ct);
        await WriteJsonRaw(db, archive, "data/providers.json", "SELECT * FROM providers", ct);
        await WriteJsonRaw(db, archive, "data/orders.json", "SELECT * FROM orders", ct);
        await WriteJsonRaw(db, archive, "data/order_employees.json", "SELECT * FROM order_employees", ct);
        await WriteJsonRaw(db, archive, "data/order_sell_invoices.json", "SELECT * FROM order_sell_invoices", ct);
        await WriteJsonRaw(db, archive, "data/order_buy_invoices.json", "SELECT * FROM order_buy_invoices", ct);
        await WriteJsonRaw(db, archive, "data/order_unique_numbers.json", "SELECT * FROM order_unique_numbers", ct);
        await WriteJsonRaw(db, archive, "data/extra_providers.json", "SELECT * FROM extra_providers", ct);
        await WriteJsonRaw(db, archive, "data/stages.json", "SELECT * FROM stages", ct);
        await WriteJsonRaw(db, archive, "data/steps.json", "SELECT * FROM steps", ct);
        await WriteJsonRaw(db, archive, "data/order_sequences.json", "SELECT * FROM order_sequences", ct);
        await WriteJsonRaw(db, archive, "data/order_step_historys.json", "SELECT * FROM order_step_historys", ct);
        await WriteJsonRaw(db, archive, "data/notes.json", "SELECT * FROM notes", ct);
        await WriteJsonRaw(db, archive, "data/order_logs.json", "SELECT * FROM order_logs", ct);
        await WriteJsonRaw(db, archive, "data/note_files.json", "SELECT * FROM note_files", ct);
        await WriteJsonRaw(db, archive, "data/currencies.json", "SELECT * FROM currencies", ct);
        await WriteJsonRaw(db, archive, "data/stored_files.json", "SELECT * FROM files", ct);
        await WriteJsonRaw(db, archive, "data/client_file_groups.json", "SELECT * FROM client_file_groups", ct);
        await WriteJsonRaw(db, archive, "data/client_file_group_items.json", "SELECT * FROM client_file_group_items", ct);
        await WriteJsonRaw(db, archive, "data/provider_file_groups.json", "SELECT * FROM provider_file_groups", ct);
        await WriteJsonRaw(db, archive, "data/provider_file_group_items.json", "SELECT * FROM provider_file_group_items", ct);
        await WriteJsonRaw(db, archive, "data/client_payments.json", "SELECT * FROM client_payments", ct);
        await WriteJsonRaw(db, archive, "data/client_payment_files.json", "SELECT * FROM client_payment_files", ct);
        await WriteJsonRaw(db, archive, "data/provider_payments.json", "SELECT * FROM provider_payments", ct);
        await WriteJsonRaw(db, archive, "data/provider_payment_files.json", "SELECT * FROM provider_payment_files", ct);
        await WriteJsonRaw(db, archive, "data/extra_provider_payments.json", "SELECT * FROM extra_provider_payments", ct);
        await WriteJsonRaw(db, archive, "data/extra_provider_payment_files.json", "SELECT * FROM extra_provider_payment_files", ct);
        await WriteJsonRaw(db, archive, "data/order_status_files.json", "SELECT * FROM order_status_files", ct);
        await WriteJsonRaw(db, archive, "data/order_log_files.json", "SELECT * FROM order_log_files", ct);
    }

    private async Task ExportCsvSummaries(AppDbContext db, ZipArchive archive, CancellationToken ct)
    {
        // Clients CSV
        var clients = await db.Clients.IgnoreQueryFilters().ToListAsync(ct);
        var clientCsv = ToCsv(clients);
        WriteText(archive, "csv/clients_summary.csv", clientCsv);

        // Providers CSV
        var providers = await db.Providers.IgnoreQueryFilters().ToListAsync(ct);
        var providerCsv = ToCsv(providers);
        WriteText(archive, "csv/providers_summary.csv", providerCsv);

        // Users/Employees CSV
        var users = await db.Users.IgnoreQueryFilters().ToListAsync(ct);
        var userCsv = ToCsv(users);
        WriteText(archive, "csv/employees_summary.csv", userCsv);

        // Orders CSV
        var orders = await db.Orders.IgnoreQueryFilters().ToListAsync(ct);
        var orderCsv = ToCsv(orders);
        WriteText(archive, "csv/orders_summary.csv", orderCsv);
    }

    private async Task ExportMinioAssets(ZipArchive archive, CancellationToken ct)
    {
        var allObjects = await _storage.ListObjectsAsync(null, ct);
        foreach (var objectName in allObjects)
        {
            // Skip backup files themselves
            if (objectName.StartsWith("system/backups/", StringComparison.OrdinalIgnoreCase))
                continue;

            try
            {
                using var ms = new MemoryStream();
                await _storage.DownloadAsync(objectName, ms, ct);
                ms.Position = 0;

                var entry = archive.CreateEntry($"assets/{objectName}", CompressionLevel.Fastest);
                await using var entryStream = entry.Open();
                await ms.CopyToAsync(entryStream, ct);
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Failed to export MinIO object: {ObjectName}", objectName);
            }
        }
    }

    private async Task WriteManifest(AppDbContext db, ZipArchive archive, string timestamp, CancellationToken ct)
    {
        var manifest = new
        {
            version = "1.0",
            createdAt = DateTime.UtcNow.ToString("o"),
            counts = new
            {
                clients = await db.Clients.IgnoreQueryFilters().CountAsync(ct),
                providers = await db.Providers.IgnoreQueryFilters().CountAsync(ct),
                users = await db.Users.IgnoreQueryFilters().CountAsync(ct),
                orders = await db.Orders.IgnoreQueryFilters().CountAsync(ct),
                storedFiles = await db.StoredFiles.IgnoreQueryFilters().CountAsync(ct)
            }
        };

        var json = JsonSerializer.Serialize(manifest, JsonOpts);
        WriteText(archive, "manifest.json", json);
    }

    private static async Task WriteJson<T>(ZipArchive archive, string path, T data)
    {
        var json = JsonSerializer.Serialize(data, JsonOpts);
        var entry = archive.CreateEntry(path, CompressionLevel.Fastest);
        await using var stream = entry.Open();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(json);
    }

    private static async Task WriteJsonRaw(AppDbContext db, ZipArchive archive, string path, string sql, CancellationToken ct)
    {
        try
        {
            var conn = db.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync(ct);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            using var reader = await cmd.ExecuteReaderAsync(ct);

            var rows = new List<Dictionary<string, object?>>();
            while (await reader.ReadAsync(ct))
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                rows.Add(row);
            }
            await reader.CloseAsync();

            var json = JsonSerializer.Serialize(rows, JsonOpts);
            var entry = archive.CreateEntry(path, CompressionLevel.Fastest);
            await using var stream = entry.Open();
            await using var writer = new StreamWriter(stream);
            await writer.WriteAsync(json);
        }
        catch (Exception)
        {
            // Table may not exist — write empty array
            var entry = archive.CreateEntry(path, CompressionLevel.Fastest);
            await using var stream = entry.Open();
            await using var writer = new StreamWriter(stream);
            await writer.WriteAsync("[]");
        }
    }

    private static string ToCsv<T>(List<T> items)
    {
        if (items.Count == 0) return "";
        var props = typeof(T).GetProperties()
            .Where(p => p.PropertyType.IsPrimitive
                || p.PropertyType == typeof(string)
                || p.PropertyType == typeof(DateTime)
                || p.PropertyType == typeof(DateTime?)
                || p.PropertyType == typeof(decimal)
                || p.PropertyType == typeof(decimal?)
                || p.PropertyType == typeof(int?)
                || p.PropertyType == typeof(bool)
                || p.PropertyType == typeof(bool?)
                || p.PropertyType.IsEnum)
            .ToArray();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine(string.Join(",", props.Select(p => p.Name)));
        foreach (var item in items)
        {
            var values = props.Select(p =>
            {
                var val = p.GetValue(item);
                if (val is null) return "";
                if (val is string s) return $"\"{s.Replace("\"", "\"\"")}\"";
                if (val is DateTime dt) return dt.ToString("o", CultureInfo.InvariantCulture);
                return val.ToString() ?? "";
            });
            sb.AppendLine(string.Join(",", values));
        }
        return sb.ToString();
    }

    private static void WriteText(ZipArchive archive, string path, string content)
    {
        var entry = archive.CreateEntry(path, CompressionLevel.Fastest);
        using var stream = entry.Open();
        using var writer = new StreamWriter(stream);
        writer.Write(content);
    }

    /// <summary>Keep only the last 10 completed export backups. Delete older ones from MinIO.</summary>
    private async Task EnforceRetentionLimit(AppDbContext db, CancellationToken ct)
    {
        const int maxBackups = 10;
        var completedExports = await db.BackupJobs
            .Where(x => x.Type == BackupJobType.Export && x.Status == BackupJobStatus.Completed)
            .OrderByDescending(x => x.CompletedAt)
            .ToListAsync(ct);

        if (completedExports.Count <= maxBackups) return;

        var toDelete = completedExports.Skip(maxBackups).ToList();
        foreach (var old in toDelete)
        {
            if (!string.IsNullOrWhiteSpace(old.FilePath))
            {
                try { await _storage.DeleteAsync(old.FilePath, ct); }
                catch (Exception ex) { _log.LogWarning(ex, "Failed to delete old backup file: {Path}", old.FilePath); }
            }
            db.BackupJobs.Remove(old);
        }
        await db.SaveChangesAsync(ct);
        _log.LogInformation("Retention cleanup: removed {Count} old backups.", toDelete.Count);
    }
}
