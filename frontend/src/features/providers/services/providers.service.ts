import { injectable } from 'tsyringe';
import { ProvidersRepository } from '../data/providers.api';
import type {
    ProviderFileGroup,
    ProviderCreateRequest,
} from '@/api/providers/types';
import { ProvidersServiceDto } from "@/features/providers/dto/providers.service.dto";

@injectable()
export class ProvidersService {
    constructor(private repo: ProvidersRepository) { }

    async list(page = 1, pageSize = 10, filters?: Record<string, any>) {
        const data = await this.repo.fetchProviders({ page, pageSize, filters });
        return {
            ...data,
            items: data.items.map((p) => ({
                id: p.id,
                name: p.name,
                email: p.email,
                website: p.website,
                phone: p.phone,
                secondPhone: p.secondPhone,
                address: p.address,
                description: p.description,
                isActive: p.isActive,
            })),
        };
    }


    async getDetails(id: number): Promise<ProvidersServiceDto> {
        const data = await this.repo.getProviderDetails(id);
        return data
    }
    private buildFormData(dto: Omit<ProviderCreateRequest, 'file'>, file?: File): FormData {
        const form = new FormData();
        form.append('Name', dto.name);
        if (dto.email) form.append('Email', dto.email);
        if (dto.website) form.append('Website', dto.website);
        if (dto.phone) form.append('Phone', dto.phone);
        if (dto.secondPhone) form.append('SecondPhone', dto.secondPhone);
        if (dto.address) form.append('Address', dto.address);
        if (dto.description) form.append('Description', dto.description);
        form.append('IsActive', String(dto.isActive ?? true));
        if (file) form.append('File', file);
        return form;
    }

    async create(dto: ProviderCreateRequest) {
        const formData = this.buildFormData(dto, dto.file);
        await this.repo.createProvider(formData);
    }

    async update(id: number, dto: ProviderCreateRequest) {
        const formData = this.buildFormData(dto, dto.file);
        await this.repo.updateProvider(id, formData);
    }

    async delete(id: number) {
        await this.repo.deleteProvider(id);
    }

    async getFileGroups(providerId: number): Promise<ProviderFileGroup[]> {
        return this.repo.fetchFileGroups(providerId);
    }

    async createFileGroup(providerId: number, payload: { label: string; files: File[] }) {
        const formData = new FormData();
        formData.append('Label', payload.label);
        payload.files.forEach((f) => formData.append('Files', f));
        return this.repo.createFileGroup(providerId, formData);
    }

    async updateFileGroupLabel(providerId: number, groupId: number, label: string) {
        await this.repo.updateFileGroupLabel(providerId, groupId, label);
    }

    async addFilesToGroup(providerId: number, groupId: number, files: File[]) {
        await this.repo.addFilesToGroup(providerId, groupId, files);
    }

    async deleteFileFromGroup(providerId: number, itemId: number) {
        await this.repo.deleteFileFromGroup(providerId, itemId);
    }

    async deleteFileGroup(providerId: number, fileGroupId: number): Promise<void> {
        await this.repo.deleteFileGroup(providerId, fileGroupId);
    }
}
