export interface ClientRecord {
    id: number;
    name: string;
    phone?: string;
    secondPhone?: string;
    email?: string;
    website?: string;
    address?: string;
    description: string;
    isActive: boolean;
}

export interface ClientListResponse {
    page: number;
    pageSize: number;
    total: number;
    items: ClientRecord[];
}

export interface ClientFileLink {
    id?: number;
    fileId?: number;
    fileName: string;
    url: string;
}

export interface ClientFileGroup {
    id: number;
    clientId: number;
    label: string;
    createdByUserId: number;
    createdAt: string;
    updatedAt: string;
    groupItems: ClientFileLink[];
}
