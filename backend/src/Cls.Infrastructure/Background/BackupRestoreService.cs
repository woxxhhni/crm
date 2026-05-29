using System.IO.Compression;
using System.Text.Json;
using Cls.Application.Abstractions;
using Cls.Domain.Enums;
using Cls.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cls.Infrastructure.Background;

/// <summary>
/// Background worker that processes pending restore (import) jobs.
/// Before restoring, it triggers an auto-backup. Then it replaces all DB records
/// and MinIO assets from the ZIP, while protecting the system/backups/ path.
/// </summary>
public class BackupRestoreService : BackgroundService
{
    private readonly ILogger<BackupRestoreService> _log;
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IObjectStorageService _storage;

    private static readonly string[] TruncateOrder = new[]
    {
        // 1. Junction/file tables (no dependents)
        "order_log_files",
        "note_files",
        "order_status_files",
        "client_payment_files",
        "provider_payment_files",
        "extra_provider_payment_files",
        "client_file_group_items",
        "provider_file_group_items",
        "order_buy_invoices",
        "order_sell_invoices",
        "order_employees",
        "order_unique_numbers",

        // 2. Child tables
        "order_logs",
        "order_step_historys",
        "notes",
        "client_payments",
        "provider_payments",
        "extra_provider_payments",
        "extra_providers",
        "client_file_groups",
        "provider_file_groups",

        // 3. Main tables
        "orders",
        "order_sequences",
        "clients",
        "providers",
        "files",
        "stages",
        "steps",
        "currencies",

        // 4. Core tables
        "users",
        "outbox_messages"
    };

    public BackupRestoreService(ILogger<BackupRestoreService> log, IDbContextFactory<AppDbContext> dbFactory, IObjectStorageService storage)
    {
        _log = log;
        _dbFactory = dbFactory;
        _storage = storage;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log.LogInformation("BackupRestoreService started.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(stoppingToken);

                var job = await db.BackupJobs
                    .Where(x => x.Status == BackupJobStatus.Pending && x.Type == BackupJobType.Import)
                    .OrderBy(x => x.CreatedAt)
                    .FirstOrDefaultAsync(stoppingToken);

                if (job is not null)
                {
                    await ProcessRestoreJob(db, job, stoppingToken);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _log.LogError(ex, "BackupRestoreService loop error.");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
        _log.LogInformation("BackupRestoreService stopped.");
    }

    private async Task ProcessRestoreJob(AppDbContext db, Domain.Entities.BackupJob job, CancellationToken ct)
    {
        job.Status = BackupJobStatus.InProgress;
        job.StartedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        _log.LogInformation("Processing restore job {JobId} from file: {File}", job.Id, job.FilePath);

        try
        {
            // Step 1: Auto-backup before restore
            _log.LogInformation("Creating auto-backup before restore...");
            await CreateAutoBackup(db, job.RequestedByUserId, ct);

            // Step 2: Download the restore ZIP from MinIO to a temp file
            if (string.IsNullOrWhiteSpace(job.FilePath))
                throw new InvalidOperationException("Restore job has no source file path.");

            var tempZipPath = Path.Combine(Path.GetTempPath(), $"restore_{job.Id}_{Guid.NewGuid():N}.zip");
            try
            {
                await using (var fileStream = File.Create(tempZipPath))
                {
                    await _storage.DownloadAsync(job.FilePath, fileStream, ct);
                }
                _log.LogInformation("Downloaded restore ZIP to temp file: {Path} ({Size} bytes)", tempZipPath, new FileInfo(tempZipPath).Length);

                using var archive = ZipFile.OpenRead(tempZipPath);

                // Step 3: Validate manifest
                var manifestEntry = archive.GetEntry("manifest.json")
                    ?? throw new InvalidOperationException("ZIP is missing manifest.json — invalid backup file.");

                using var manifestReader = new StreamReader(manifestEntry.Open());
                var manifestJson = await manifestReader.ReadToEndAsync(ct);
                var manifest = JsonSerializer.Deserialize<JsonElement>(manifestJson);
                var version = manifest.GetProperty("version").GetString();
                if (version != "1.0")
                    throw new InvalidOperationException($"Unsupported backup version: {version}. Expected 1.0.");

                _log.LogInformation("Backup manifest validated. Version: {Version}", version);

                // Step 4: Restore database
                await RestoreDatabase(db, archive, ct);

                // Step 5: Restore MinIO assets
                await RestoreMinioAssets(archive, ct);

                // Step 6: Mark job as completed
                await using var freshDb = await _dbFactory.CreateDbContextAsync(ct);
                var freshJob = await freshDb.BackupJobs.FirstOrDefaultAsync(x => x.Id == job.Id, ct);
                if (freshJob is not null)
                {
                    freshJob.Status = BackupJobStatus.Completed;
                    freshJob.CompletedAt = DateTime.UtcNow;
                    await freshDb.SaveChangesAsync(ct);
                }

                _log.LogInformation("Restore job {JobId} completed successfully.", job.Id);
            }
            finally
            {
                // Clean up temp file
                try { if (File.Exists(tempZipPath)) File.Delete(tempZipPath); }
                catch { /* best effort */ }
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Restore job {JobId} failed.", job.Id);

            try
            {
                await using var errDb = await _dbFactory.CreateDbContextAsync(ct);
                var errJob = await errDb.BackupJobs.FirstOrDefaultAsync(x => x.Id == job.Id, ct);
                if (errJob is not null)
                {
                    errJob.Status = BackupJobStatus.Failed;
                    errJob.ErrorMessage = ex.Message.Length > 4000 ? ex.Message[..4000] : ex.Message;
                    errJob.CompletedAt = DateTime.UtcNow;
                    await errDb.SaveChangesAsync(ct);
                }
            }
            catch (Exception innerEx)
            {
                _log.LogError(innerEx, "Failed to update restore job status after error.");
            }
        }
    }

    private async Task CreateAutoBackup(AppDbContext db, int userId, CancellationToken ct)
    {
        var autoJob = new Domain.Entities.BackupJob
        {
            Type = BackupJobType.Export,
            Status = BackupJobStatus.Pending,
            RequestedByUserId = userId,
            CreatedByUserId = userId,
            UpdatedByUserId = userId
        };
        db.BackupJobs.Add(autoJob);
        await db.SaveChangesAsync(ct);

        // Wait for the export service to pick up and complete the auto-backup
        var timeout = DateTime.UtcNow.AddMinutes(10);
        while (DateTime.UtcNow < timeout)
        {
            await Task.Delay(TimeSpan.FromSeconds(3), ct);
            await using var checkDb = await _dbFactory.CreateDbContextAsync(ct);
            var status = await checkDb.BackupJobs
                .Where(x => x.Id == autoJob.Id)
                .Select(x => x.Status)
                .FirstOrDefaultAsync(ct);

            if (status == BackupJobStatus.Completed)
            {
                _log.LogInformation("Auto-backup {JobId} completed before restore.", autoJob.Id);
                return;
            }

            if (status == BackupJobStatus.Failed)
                throw new InvalidOperationException($"Auto-backup (job {autoJob.Id}) failed. Aborting restore.");
        }

        throw new TimeoutException("Auto-backup timed out after 10 minutes. Aborting restore.");
    }

    private async Task RestoreDatabase(AppDbContext db, ZipArchive archive, CancellationToken ct)
    {
        _log.LogInformation("Starting database restore...");
        var conn = db.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open)
            await conn.OpenAsync(ct);

        // Collect backup_jobs data BEFORE truncation (we need to preserve them)
        var backupJobsData = new List<Dictionary<string, object?>>();
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT * FROM backup_jobs";
            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                backupJobsData.Add(row);
            }
        }

        await using var tx = await conn.BeginTransactionAsync(ct);

        try
        {
            // Disable FK constraints
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = "SET session_replication_role = 'replica';";
                await cmd.ExecuteNonQueryAsync(ct);
            }

            // Truncate ALL tables at once in a single statement (avoids FK cascade issues)
            var allTables = string.Join(", ", TruncateOrder.Select(t => $"\"{t}\""));
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = $"TRUNCATE TABLE {allTables} CASCADE;";
                await cmd.ExecuteNonQueryAsync(ct);
            }
            _log.LogInformation("All tables truncated successfully.");

            // Import each JSON data file
            var dataEntries = archive.Entries
                .Where(e => e.FullName.StartsWith("data/") && e.FullName.EndsWith(".json"))
                .ToList();

            foreach (var entry in dataEntries)
            {
                await ImportJsonToTable(conn, tx, entry, ct);
            }

            // Re-enable FK constraints
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = "SET session_replication_role = 'origin';";
                await cmd.ExecuteNonQueryAsync(ct);
            }

            // Reset all sequences
            await ResetSequences(conn, tx, ct);

            // Restore backup_jobs records (they were preserved)
            await RestoreBackupJobs(conn, tx, backupJobsData, ct);

            // Reset backup_jobs sequence since we just re-inserted rows
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = "SELECT setval(pg_get_serial_sequence('backup_jobs', 'id'), COALESCE((SELECT MAX(id) FROM backup_jobs), 1));";
                await cmd.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);
            _log.LogInformation("Database restore completed.");
        }
        catch
        {
            await tx.RollbackAsync(ct);
            // Re-enable FK constraints even on failure
            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SET session_replication_role = 'origin';";
                await cmd.ExecuteNonQueryAsync(ct);
            }
            catch { /* best effort */ }
            throw;
        }
    }

    private async Task ImportJsonToTable(System.Data.Common.DbConnection conn, System.Data.Common.DbTransaction tx,
        ZipArchiveEntry entry, CancellationToken ct)
    {
        var tableName = MapFileToTable(Path.GetFileNameWithoutExtension(entry.Name));
        if (tableName is null)
        {
            _log.LogWarning("Unknown data file: {File}, skipping.", entry.FullName);
            return;
        }

        using var reader = new StreamReader(entry.Open());
        var json = await reader.ReadToEndAsync(ct);
        var rows = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(json);

        if (rows is null || rows.Count == 0)
        {
            _log.LogInformation("Empty data for table {Table}, skipping.", tableName);
            return;
        }

        var imported = 0;
        foreach (var row in rows)
        {
            var columns = row.Keys.ToList();
            var colNames = string.Join(", ", columns.Select(c => $"\"{c}\""));
            var paramNames = string.Join(", ", columns.Select((_, i) => $"@p{i}"));

            // Use SAVEPOINT so a failed INSERT doesn't abort the entire transaction
            using var spCmd = conn.CreateCommand();
            spCmd.Transaction = tx;
            spCmd.CommandText = $"SAVEPOINT sp_row;";
            await spCmd.ExecuteNonQueryAsync(ct);

            try
            {
                using var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = $"INSERT INTO \"{tableName}\" ({colNames}) VALUES ({paramNames}) ON CONFLICT DO NOTHING;";

                for (int i = 0; i < columns.Count; i++)
                {
                    var param = cmd.CreateParameter();
                    param.ParameterName = $"@p{i}";
                    param.Value = ConvertJsonElement(row[columns[i]]);
                    cmd.Parameters.Add(param);
                }

                await cmd.ExecuteNonQueryAsync(ct);

                using var relCmd = conn.CreateCommand();
                relCmd.Transaction = tx;
                relCmd.CommandText = "RELEASE SAVEPOINT sp_row;";
                await relCmd.ExecuteNonQueryAsync(ct);
                imported++;
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Insert into {Table} failed for a row, rolling back to savepoint.", tableName);
                using var rbCmd = conn.CreateCommand();
                rbCmd.Transaction = tx;
                rbCmd.CommandText = "ROLLBACK TO SAVEPOINT sp_row;";
                await rbCmd.ExecuteNonQueryAsync(ct);
            }
        }

        _log.LogInformation("Imported {Count}/{Total} rows into {Table}.", imported, rows.Count, tableName);
    }

    private static string? MapFileToTable(string fileName) => fileName switch
    {
        "users" => "users",
        "clients" => "clients",
        "providers" => "providers",
        "orders" => "orders",
        "order_employees" => "order_employees",
        "order_sell_invoices" => "order_sell_invoices",
        "order_buy_invoices" => "order_buy_invoices",
        "order_unique_numbers" => "order_unique_numbers",
        "extra_providers" => "extra_providers",
        "stages" => "stages",
        "steps" => "steps",
        "order_sequences" => "order_sequences",
        "order_step_historys" => "order_step_historys",
        "notes" => "notes",
        "order_logs" => "order_logs",
        "note_files" => "note_files",
        "currencies" => "currencies",
        "stored_files" => "files",
        "client_file_groups" => "client_file_groups",
        "client_file_group_items" => "client_file_group_items",
        "provider_file_groups" => "provider_file_groups",
        "provider_file_group_items" => "provider_file_group_items",
        "client_payments" => "client_payments",
        "client_payment_files" => "client_payment_files",
        "provider_payments" => "provider_payments",
        "provider_payment_files" => "provider_payment_files",
        "extra_provider_payments" => "extra_provider_payments",
        "extra_provider_payment_files" => "extra_provider_payment_files",
        "order_status_files" => "order_status_files",
        "order_log_files" => "order_log_files",
        _ => null
    };

    private static object ConvertJsonElement(JsonElement el) => el.ValueKind switch
    {
        JsonValueKind.String => el.TryGetDateTime(out var dt) ? dt : el.GetString()!,
        JsonValueKind.Number => el.TryGetInt64(out var l) ? l : el.GetDecimal(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Null => DBNull.Value,
        JsonValueKind.Undefined => DBNull.Value,
        JsonValueKind.Array or JsonValueKind.Object => el.GetRawText(),
        _ => el.GetRawText()
    };

    private async Task ResetSequences(System.Data.Common.DbConnection conn, System.Data.Common.DbTransaction tx, CancellationToken ct)
    {
        _log.LogInformation("Resetting PostgreSQL sequences...");
        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        // This PostgreSQL query resets all identity/serial sequences to MAX(id) + 1
        cmd.CommandText = @"
            DO $$
            DECLARE
                r RECORD;
                max_val BIGINT;
            BEGIN
                FOR r IN
                    SELECT c.oid::regclass AS tbl, a.attname AS col,
                           pg_get_serial_sequence(c.oid::regclass::text, a.attname) AS seq
                    FROM pg_class c
                    JOIN pg_attribute a ON a.attrelid = c.oid
                    WHERE c.relkind = 'r'
                      AND a.attnum > 0
                      AND NOT a.attisdropped
                      AND pg_get_serial_sequence(c.oid::regclass::text, a.attname) IS NOT NULL
                LOOP
                    EXECUTE format('SELECT COALESCE(MAX(%I), 0) FROM %s', r.col, r.tbl) INTO max_val;
                    EXECUTE format('SELECT setval(%L, %s)', r.seq, GREATEST(max_val + 1, 1));
                END LOOP;
            END $$;";
        await cmd.ExecuteNonQueryAsync(ct);
        _log.LogInformation("Sequences reset completed.");
    }

    private async Task RestoreBackupJobs(System.Data.Common.DbConnection conn, System.Data.Common.DbTransaction tx,
        List<Dictionary<string, object?>> rows, CancellationToken ct)
    {
        if (rows.Count == 0) return;

        foreach (var row in rows)
        {
            var columns = row.Keys.ToList();
            var colNames = string.Join(", ", columns.Select(c => $"\"{c}\""));
            var paramNames = string.Join(", ", columns.Select((_, i) => $"@p{i}"));

            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = $"INSERT INTO \"backup_jobs\" ({colNames}) VALUES ({paramNames}) ON CONFLICT (id) DO NOTHING;";

            for (int i = 0; i < columns.Count; i++)
            {
                var param = cmd.CreateParameter();
                param.ParameterName = $"@p{i}";
                param.Value = row[columns[i]] ?? DBNull.Value;
                cmd.Parameters.Add(param);
            }

            try { await cmd.ExecuteNonQueryAsync(ct); }
            catch (Exception ex) { _log.LogWarning(ex, "Restore backup_jobs row failed, skipping."); }
        }
    }

    private async Task RestoreMinioAssets(ZipArchive archive, CancellationToken ct)
    {
        _log.LogInformation("Starting MinIO asset restore...");

        // Delete all objects EXCEPT system/backups/
        await _storage.DeleteByPrefixAsync("", excludePrefix: "system/backups/", ct);

        // Upload all assets from the ZIP
        var assetEntries = archive.Entries
            .Where(e => e.FullName.StartsWith("assets/") && e.Length > 0)
            .ToList();

        var count = 0;
        foreach (var entry in assetEntries)
        {
            // "assets/clients/profiles/abc.jpg" → "clients/profiles/abc.jpg"
            var objectName = entry.FullName["assets/".Length..];
            if (string.IsNullOrWhiteSpace(objectName)) continue;

            try
            {
                using var entryStream = entry.Open();
                using var ms = new MemoryStream();
                await entryStream.CopyToAsync(ms, ct);
                ms.Position = 0;

                var contentType = GuessContentType(objectName);
                await _storage.UploadAsync(objectName, ms, contentType, ms.Length, ct);
                count++;
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Failed to restore MinIO asset: {Name}", objectName);
            }
        }

        _log.LogInformation("MinIO asset restore completed. Restored {Count} files.", count);
    }

    private static string GuessContentType(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".zip" => "application/zip",
            ".csv" => "text/csv",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }
}
