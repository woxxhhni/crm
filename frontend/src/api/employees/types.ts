export interface EmployeeRecord {
    id: number;
    name: string;
    role: 'Manager' | 'Employee';
    phone: string;
    email: string;
    address: string;
    description: string;
    isActive: boolean;
    userProfileUrl?: string;
}

export interface EmployeeListResponse {
    page: number;
    pageSize: number;
    total: number;
    items: EmployeeRecord[];
}

export interface ResetPasswordPayload {
    newPassword: string;
    confirmPassword: string;
}
