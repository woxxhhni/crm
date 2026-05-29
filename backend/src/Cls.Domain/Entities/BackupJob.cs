using Cls.Domain.Common;
using Cls.Domain.Enums;

namespace Cls.Domain.Entities;

public class BackupJob : BaseEntity
{
    public BackupJobStatus Status { get; set; } = BackupJobStatus.Pending;
    public BackupJobType Type { get; set; }
    public string? FileName { get; set; }
    public string? FilePath { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int RequestedByUserId { get; set; }
    public User RequestedByUser { get; set; } = default!;
}
