
export interface OrderServiceDTO {
    id?: number;
    OrderDate: string;
    title: string;
    description?: string;
    providerId: string | number;
    clientId: string | number;
    buyCurrency: string;
    buyAmount?: string|number;
    sellCurrency: string;
    sellAmount?: string|number;
    assignedByUserId?: number[];
    buyInvoiceFiles?: (File | number)[]; // Can contain both new File objects and existing file IDs
    sellInvoiceFiles?: (File | number)[]; // Can contain both new File objects and existing file IDs
    removedBuyFileIds?: number[];
    removedSellFileIds?: number[];
}
