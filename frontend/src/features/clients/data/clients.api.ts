import {HttpClientInstance} from '@/services/http-client.service';
import {ClientEndpoints} from '@/api/clients/endpoints';
import {ClientFileGroup, ClientListResponse, ClientRecord} from '@/api/clients/types';

export class ClientsRepository {
    async fetchClients(params?: {
        page?: number;
        pageSize?: number;
        filters?: Record<string, any>;
    }): Promise<ClientListResponse> {
        const { page, pageSize, filters } = params || {};

        const queryParams: Record<string, any> = {
            page,
            pageSize,
            ...filters,
        };

        Object.keys(queryParams).forEach((key) => {
            if (
                queryParams[key] === undefined ||
                queryParams[key] === null ||
                queryParams[key] === ''
            ) {
                delete queryParams[key];
            }
        });
        const { data } = await HttpClientInstance.get<ClientListResponse>(
            ClientEndpoints.root,
            queryParams
        );

        return data;
    }


    async getClientDetails(id: number): Promise<ClientRecord> {
        const {data} = await HttpClientInstance.get<ClientRecord>(ClientEndpoints.byId(id));
        return data;
    }

    async createClient(payload: FormData): Promise<void> {
        await HttpClientInstance.post(ClientEndpoints.root, payload, {
            headers: {'Content-Type': 'multipart/form-data'},
        });
    }

    async updateClient(id: number, payload: FormData): Promise<void> {
        await HttpClientInstance.put(ClientEndpoints.byId(id), payload, {
            headers: {'Content-Type': 'multipart/form-data'},
        });
    }

    async deleteClient(id: number): Promise<void> {
        await HttpClientInstance.delete(ClientEndpoints.byId(id));
    }

    async fetchFileGroups(clientId: number): Promise<ClientFileGroup[]> {
        const {data} = await HttpClientInstance.get<ClientFileGroup[]>(
            ClientEndpoints.fileGroups(clientId)
        );
        return data;
    }

    async createFileGroup(clientId: number, formData: FormData) {
        const {data} = await HttpClientInstance.post(
            ClientEndpoints.fileGroups(clientId),
            formData,
            {
                headers: {'Content-Type': 'multipart/form-data'},
            }
        );
        return data;
    }

    async updateFileGroupLabel(clientId: number, fileGroupId: number, label: string) {
        const body = { label };

        const { data } = await HttpClientInstance.put(
            ClientEndpoints.filesGroupAction(clientId, fileGroupId),
            body,
            {
                headers: { 'Content-Type': 'application/json' },
            }
        );

        return data;
    }

    async addFilesToGroup(clientId: number, fileGroupId: number, files: File[]) {
        const formData = new FormData();

        files.forEach((file) => {
            if (file instanceof File) {
                formData.append('Files', file);
            }
        });

        const { data } = await HttpClientInstance.post(
            ClientEndpoints.filesGroupItemAction(clientId, fileGroupId),
            formData,
            {
                headers: { 'Content-Type': 'multipart/form-data' },
            }
        );

        return data;
    }

    async deleteFileFromGroup(clientId: number, itemId: number) {
        const { data } = await HttpClientInstance.delete(
            ClientEndpoints.filesGroupItemDelete(clientId, itemId)
        );
        return data;
    }

    async deleteFileGroup(clientId: number, fileGroupId: number) {
        const { data } = await HttpClientInstance.delete(
            ClientEndpoints.filesGroupAction(clientId, fileGroupId)
        );
        return data;
    }
}
