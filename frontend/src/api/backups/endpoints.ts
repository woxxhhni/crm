export const BackupEndpoints = {
    root: '/api/v1/Backups',
    byId: (id: number) => `/api/v1/Backups/${id}`,
    download: (id: number) => `/api/v1/Backups/${id}/download`,
    restore: (id: number) => `/api/v1/Backups/${id}/restore`,
    uploadRestore: '/api/v1/Backups/upload-restore',
} as const;
