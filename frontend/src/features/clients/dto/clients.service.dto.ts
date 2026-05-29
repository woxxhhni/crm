export interface ClientServiceDTO {
    id: number;
    name: string;
    email?: string;
    website?: string;
    phone?: string;
    secondPhone?: string;
    address?: string;
    description?: string;
    profileUrl?: string,
    isActive?: boolean;
    file?: File;
}
