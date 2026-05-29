namespace Cls.Domain.Enums;

public enum BackupJobStatus
{
    Pending,
    InProgress,
    Completed,
    Failed
}

public enum BackupJobType
{
    Export,
    Import
}
