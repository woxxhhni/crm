import { HttpClientInstance } from '@/services/http-client.service';
import { BackupEndpoints } from '@/api/backups/endpoints';
import { BackupJob, BackupCreateResponse, BackupDownloadResponse } from '@/api/backups/types';

export class BackupsRepository {
    async fetchAll(): Promise<BackupJob[]> {
        const { data } = await HttpClientInstance.get<BackupJob[]>(BackupEndpoints.root);
        return data;
    }

    async getById(id: number): Promise<BackupJob> {
        const { data } = await HttpClientInstance.get<BackupJob>(BackupEndpoints.byId(id));
        return data;
    }

    async createBackup(): Promise<BackupCreateResponse> {
        const { data } = await HttpClientInstance.post<BackupCreateResponse>(BackupEndpoints.root);
        return data;
    }

    async getDownloadUrl(id: number): Promise<BackupDownloadResponse> {
        const { data } = await HttpClientInstance.get<BackupDownloadResponse>(BackupEndpoints.download(id));
        return data;
    }

    async restoreFromBackup(id: number): Promise<BackupCreateResponse> {
        const { data } = await HttpClientInstance.post<BackupCreateResponse>(BackupEndpoints.restore(id));
        return data;
    }

    async uploadAndRestore(file: File): Promise<BackupCreateResponse> {
        const formData = new FormData();
        formData.append('file', file);
        const { data } = await HttpClientInstance.post<BackupCreateResponse>(
            BackupEndpoints.uploadRestore,
            formData,
            { headers: { 'Content-Type': 'multipart/form-data' } }
        );
        return data;
    }

    async deleteBackup(id: number): Promise<void> {
        await HttpClientInstance.delete(BackupEndpoints.byId(id));
    }
}
