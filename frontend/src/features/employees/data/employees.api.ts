import { HttpClientInstance } from '@/services/http-client.service';
import { EmployeesEndpoints } from '@/api/employees/endpoints';
import { EmployeeListResponse, EmployeeRecord, ResetPasswordPayload } from '@/api/employees/types';
import {EmployeeServiceDTO} from '../dto/employees.service.dto';

export class EmployeesRepository {
    async fetchEmployees(params?: { page?: number; pageSize?: number }): Promise<EmployeeListResponse> {
        const { data } = await HttpClientInstance.get<EmployeeListResponse>(
            EmployeesEndpoints.root,
            params
        );
        return data;
    }

    async getEmployeeDetails(id: number|string): Promise<EmployeeRecord> {
        const { data } = await HttpClientInstance.get<EmployeeRecord>(EmployeesEndpoints.byId(id));
        return data;
    }

    async createEmployee(payload: FormData): Promise<void> {
        await HttpClientInstance.post(EmployeesEndpoints.root, payload, {
            headers: { 'Content-Type': 'multipart/form-data' },
        });
    }

    async updateEmployee(id: number, payload: FormData): Promise<void> {
        await HttpClientInstance.put(EmployeesEndpoints.byId(id), payload, {
            headers: { 'Content-Type': 'multipart/form-data' },
        });
    }

    async deleteEmployee(id: number): Promise<void> {
        await HttpClientInstance.delete(EmployeesEndpoints.byId(id));
    }

    async resetPassword(id: number, payload: ResetPasswordPayload): Promise<void> {
        await HttpClientInstance.put(EmployeesEndpoints.resetPassword(id), payload, {
            headers: { 'Content-Type': 'application/json' },
        });
    }
}
