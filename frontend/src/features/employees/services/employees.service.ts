import { injectable } from 'tsyringe';
import { EmployeesRepository } from '../data/employees.api';
import type { EmployeeServiceDTO } from '@/features/employees/dto/employees.service.dto';
import { EmployeeRecord, ResetPasswordPayload } from '@/api/employees/types';

@injectable()
export class EmployeesService {
    constructor(private repo: EmployeesRepository) {}

    async list(page?: number, pageSize?: number): Promise<{
        items: EmployeeRecord[];
        page: number;
        pageSize: number;
        total: number;
    }> {
        const data = await this.repo.fetchEmployees({ page, pageSize });
        return {
            page: data.page,
            pageSize: data.pageSize,
            total: data.total,
            items: data.items.map((e) => ({
                id: e.id,
                name: e.name,
                role: e.role,
                email: e.email,
                phone: e.phone,
                address: e.address,
                description: e.description,
                isActive: e.isActive,
                profileUrl: e.userProfileUrl,
            })),
        };
    }

    async getDetails(id: number|string): Promise<EmployeeRecord> {
        const data = await this.repo.getEmployeeDetails(id);
        return data;
    }

    private buildFormData(dto: Omit<EmployeeServiceDTO, 'id'>, file?: File): FormData {
        const formData = new FormData();
        formData.append('Name', dto.name);
        formData.append('Email', dto.email);
        formData.append('Role', dto.role);
        formData.append('Phone', dto.phone);
        formData.append('Address', dto.address);
        formData.append('Password', dto.password ?? '123456');
        if (dto.description) formData.append('Description', dto.description);
        formData.append('IsActive', String(dto.isActive ?? true));
        if (file) formData.append('File', file);

        return formData;
    }

    async create(dto: Omit<EmployeeServiceDTO, 'id'> & { file?: File }) {
        if (!dto.password) {
            dto.password = '123456';
        }
        const formData = this.buildFormData(dto, dto.file);
        await this.repo.createEmployee(formData);
    }

    async update(id: number, dto: Omit<EmployeeServiceDTO, 'id'> & { file?: File }) {
        const formData = this.buildFormData(dto, dto.file);
        await this.repo.updateEmployee(id, formData);
    }

    async delete(id: number) {
        await this.repo.deleteEmployee(id);
    }

    async resetPassword(id: number, newPassword: string, confirmPassword: string) {
        const payload: ResetPasswordPayload = {
            newPassword,
            confirmPassword,
        };
        await this.repo.resetPassword(id, payload);
    }
}
