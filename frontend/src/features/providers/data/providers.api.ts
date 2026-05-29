import { HttpClientInstance } from '@/services/http-client.service';
import {
    ProviderListResponse,
    ProviderFileGroup, ProviderRecord,
} from '@/api/providers/types';
import { ProvidersEndpoints } from '@/api/providers/endpoints';

export class ProvidersRepository {
    async fetchProviders(params?: { page?: number; pageSize?: number; filters?: Record<string, any> }): Promise<ProviderListResponse> {
        const { page, pageSize, filters } = params || {};

        const queryParams: Record<string, any> = { page, pageSize, ...filters };

        Object.keys(queryParams).forEach(key => {
            if (queryParams[key] === undefined || queryParams[key] === null || queryParams[key] === '') {
                delete queryParams[key];
            }
        });

        const { data } = await HttpClientInstance.get<ProviderListResponse>(
            ProvidersEndpoints.root,
            queryParams
        );

        return data;
    }


    async getProviderDetails(id: number): Promise<ProviderRecord> {
        const { data } = await HttpClientInstance.get<ProviderRecord>(ProvidersEndpoints.byId(id));
        return data;
    }

    async createProvider(payload: FormData) {
        await HttpClientInstance.post(ProvidersEndpoints.root, payload, {
            headers: { 'Content-Type': 'multipart/form-data' },
        });
    }

    async updateProvider(id: number|string, payload: FormData) {
        await HttpClientInstance.put(ProvidersEndpoints.byId(id), payload, {
            headers: { 'Content-Type': 'multipart/form-data' },
        });
    }

    async deleteProvider(id: number) {
        await HttpClientInstance.delete(ProvidersEndpoints.byId(id));
    }

    async fetchFileGroups(providerId: number): Promise<ProviderFileGroup[]> {
        const { data } = await HttpClientInstance.get(
            ProvidersEndpoints.fileGroups(providerId)
        );
        return data as ProviderFileGroup[];
    }
    async createFileGroup(providerId: number, formData: FormData) {
        const { data } = await HttpClientInstance.post(
            ProvidersEndpoints.fileGroups(providerId),
            formData,
            { headers: { 'Content-Type': 'multipart/form-data' } }
        );
        return data;
    }

    async updateFileGroupLabel(providerId: number, fileGroupId: number, label: string) {
        const body = { label };
        const { data } = await HttpClientInstance.put(
            ProvidersEndpoints.filesGroupAction(providerId, fileGroupId),
            body,
            { headers: { 'Content-Type': 'application/json' } }
        );
        return data;
    }

    async addFilesToGroup(providerId: number, fileGroupId: number, files: File[]) {
        const formData = new FormData();
        files.forEach((f) => formData.append('Files', f));

        const { data } = await HttpClientInstance.post(
            ProvidersEndpoints.filesGroupItemAction(providerId, fileGroupId),
            formData,
            { headers: { 'Content-Type': 'multipart/form-data' } }
        );
        return data;
    }

    async deleteFileFromGroup(providerId: number, itemId: number) {
        const { data } = await HttpClientInstance.delete(
            ProvidersEndpoints.filesGroupItemDelete(providerId, itemId)
        );
        return data;
    }

    async deleteFileGroup(providerId: number, fileGroupId: number) {
        const { data } = await HttpClientInstance.delete(
            ProvidersEndpoints.filesGroupAction(providerId, fileGroupId)
        );
        return data;
    }
}
