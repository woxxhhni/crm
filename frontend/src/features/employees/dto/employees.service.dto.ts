export interface EmployeeServiceDTO {
    id?: number;
    name: string;
    role: 'manager' | 'employee';
    phone: string;
    email: string;
    address: string;
    description?: string;
    profileUrl?: string;
    isActive?: boolean;
    file?: File;
    password?: string;
}
