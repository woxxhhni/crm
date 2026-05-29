import { injectable } from 'tsyringe';
import { ClientsRepository } from '../data/clients.api';
import type { ClientServiceDTO } from '@/features/clients/dto/clients.service.dto.ts';
import { ClientRecord } from "@/api/clients/types";

@injectable()
export class ClientsService {
    constructor(private repo: ClientsRepository) { }

    async list(
        page?: number,
        pageSize?: number,
        filters?: Record<string, any>
    ): Promise<{
        items: ClientRecord[];
        page: number;
        pageSize: number;
        total: number;
    }> {
        const data = await this.repo.fetchClients({ page, pageSize, filters });

        return {
            page: data.page,
            pageSize: data.pageSize,
            total: data.total,
            items: data.items.map((c) => ({
                id: c.id,
                name: c.name,
                email: c.email,
                website: c.website,
                phone: c.phone,
                secondPhone: c.secondPhone,
                address: c.address,
                description: c.description,
                isActive: c.isActive,
            })),
        };
    }



    async getDetails(id: number): Promise<ClientServiceDTO> {
        const data = await this.repo.getClientDetails(id);
        return data
    }

    private buildFormData(dto: Omit<ClientServiceDTO, 'id'>, file?: File): FormData {
        const formData = new FormData();
        formData.append('Name', dto.name);
        if (dto.email) formData.append('Email', dto.email);
        if (dto.website) formData.append('Website', dto.website);
        if (dto.phone) formData.append('Phone', dto.phone);
        if (dto.secondPhone) formData.append('SecondPhone', dto.secondPhone);
        if (dto.address) formData.append('Address', dto.address);
        if (dto.description) formData.append('Description', dto.description);
        formData.append('IsActive', String(dto.isActive ?? true));
        if (file) formData.append('File', file);

        return formData;
    }

    async create(dto: Omit<ClientServiceDTO, 'id'> & { file?: File }) {
        const formData = this.buildFormData(dto, dto.file);
        await this.repo.createClient(formData);
    }

    async update(id: number, dto: Omit<ClientServiceDTO, 'id'> & { file?: File }) {
        const formData = this.buildFormData(dto, dto.file);
        await this.repo.updateClient(id, formData);
    }

    async delete(id: number) {
        await this.repo.deleteClient(id);
    }

    async getFileGroups(clientId: number) {
        return await this.repo.fetchFileGroups(clientId);
    }

    async createFileGroup(clientId: number, payload: { label: string; files: File[] }) {
        const formData = new FormData();
        formData.append('Label', payload.label);
        payload.files?.forEach(file => {
            if (file instanceof File) {
                formData.append('Files', file);
            }
        });
        return this.repo.createFileGroup(clientId, formData);
    }

    async updateFileGroupLabel(
        clientId: number,
        fileGroupId: number,
        label: string
    ): Promise<void> {
        await this.repo.updateFileGroupLabel(clientId, fileGroupId, label);
    }

    async addFilesToGroup(clientId: number, fileGroupId: number, files: File[]): Promise<void> {
        await this.repo.addFilesToGroup(clientId, fileGroupId, files);
    }

    async deleteFileFromGroup(clientId: number, itemId: number): Promise<void> {
        await this.repo.deleteFileFromGroup(clientId, itemId);
    }

    async deleteFileGroup(clientId: number, fileGroupId: number): Promise<void> {
        await this.repo.deleteFileGroup(clientId, fileGroupId);
    }
}
