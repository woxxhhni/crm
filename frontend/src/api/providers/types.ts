export interface ProviderRecord {
    id: number;
    name: string;
    phone?: string;
    secondPhone: string;
    email?: string;
    website?: string;
    address?: string;
    description: string;
    isActive: boolean;
    avatarUrl?: string;
}

export interface ProviderListResponse {
    page: number;
    pageSize: number;
    total: number;
    items: ProviderRecord[];
}

export interface ProviderFileLink {
    id: number;
    fileId?: number;
    fileName: string;
    url: string;
}

export interface ProviderFileGroup {
    id: number;
    providerId: number;
    label: string;
    createdByUserId: number;
    createdAt: string;
    updatedAt: string;
    groupItems: ProviderFileLink[];
}

export interface ProviderCreateRequest {
    name: string;
    phone?: string;
    secondPhone?: string;
    email?: string;
    website?: string;
    address?: string;
    description?: string;
    isActive: boolean;
    file?: File;
    avatarFile?: File
}
