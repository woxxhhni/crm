export interface BackupJob {
    id: number;
    status: BackupJobStatus;
    type: BackupJobType;
    fileName: string | null;
    fileSizeBytes: number | null;
    errorMessage: string | null;
    startedAt: string | null;
    completedAt: string | null;
    createdAt: string;
    requestedByUserId: number;
}

export enum BackupJobStatus {
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Failed = 3,
}

export enum BackupJobType {
    Export = 0,
    Import = 1,
}

export interface BackupDownloadResponse {
    url: string;
    fileName: string;
    fileSizeBytes: number;
}

export interface BackupCreateResponse {
    id: number;
    status: number;
    message: string;
}
