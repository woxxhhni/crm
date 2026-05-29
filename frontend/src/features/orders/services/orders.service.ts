import { injectable } from 'tsyringe';
import { OrdersRepository } from '../data/orders.api';
import { OrderServiceDTO } from "@/features/orders/dto/orders.service.dto";
import { OrderRecord, PaymentsListResponse, PaymentDetailResponse, PaymentCreateDTO, PaymentUpdateDTO, StepOption, NoteDetailResponse, OrdersSummaryResponse, ExtraProviderCreateDTO, ExtraProviderPaymentSummaryResponse, ExtraProviderPaymentDetailResponse } from "@/api/orders/types";

@injectable()
export class OrdersService {
    constructor(private repo: OrdersRepository) { }

    async list(page?: number, pageSize?: number, filters?: Record<string, any>): Promise<{
        items: OrderRecord[];
        page: number;
        pageSize: number;
        total: number;
    }> {
        const data = await this.repo.fetchOrders({ page, pageSize, filters });
        return {
            page: data.page,
            pageSize: data.pageSize,
            total: data.total,
            items: data.items.map((order) => ({
                id: order.id,
                orderNumber: order.orderNumber,
                orderDate: order.orderDate,
                createdAt: order.createdAt,
                title: order.title,
                clientName: order.clientName,
                providerName: order.providerName,
                status: order.status,
            })),
        };
    }
    async getDetails(orderId: number | string): Promise<any> {
        const data = await this.repo.fetchOrderDetails(orderId);
        return data
    }

    async providerOptions(): Promise<{ label: string; value: number | string }[]> {
        const providers = await this.repo.fetchProviders();
        return providers?.items?.map((provider: any) => ({
            label: provider.name,
            value: provider.id,
        })) || [];
    }
    async statuseOptions(): Promise<{ label: string; value: number | string }[]> {
        const statuses = await this.repo.fetchStatuses();
        if (!statuses) return [];
        return Object.entries(statuses).map(([key, value]) => ({
            label: String(value),
            value: Number(key),
        }));
    }

    async clientOptions(): Promise<{ label: string; value: number | string }[]> {
        const clients = await this.repo.fetchClients();
        return clients?.items?.map((client: any) => ({
            label: client.name,
            value: client.id,
        })) || [];
    }
    async currenciesOptions(): Promise<{ label: string; value: number | string }[]> {
        const currencies = await this.repo.fetchCurrencies();
        return currencies?.map((currency: any) => ({
            label: currency.name,
            value: currency.code,
        })) || [];
    }
    async employeeOptions(): Promise<{ label: string; value: number | string }[]> {
        const employees = await this.repo.fetchEmployees();
        return employees?.items?.map((employee: any) => ({
            label: employee.name,
            value: employee.id,
        })) || [];
    }

    async deleteOrder(orderId: number | string) {
        await this.repo.deleteOrder(orderId);
    }

    async delete(orderId: number | string, label: string) {
        await this.repo.deleteOrderUniqueNumber(orderId, label);
    }

    async deleteEmployee(orderId: number | string, employeeId: number | string,) {
        await this.repo.deleteOrderEmployee(orderId, employeeId);
    }

    buildFormData(dto: OrderServiceDTO) {
        const formData = new FormData();

        formData.append('OrderDate', dto.OrderDate);
        formData.append('Title', dto.title);
        formData.append('Description', dto.description || '');
        formData.append('ProviderId', String(dto.providerId));
        formData.append('ClientId', String(dto.clientId));
        formData.append('BuyCurrency', dto.buyCurrency);
        formData.append('SellCurrency', dto.sellCurrency);
        formData.append('BuyAmount', String(dto.buyAmount || 0));
        formData.append('SellAmount', String(dto.sellAmount || 0));

        // Add buy invoice files (both file IDs and new files)
        if (dto.buyInvoiceFiles && dto.buyInvoiceFiles.length > 0) {
            dto.buyInvoiceFiles.forEach(item => {
                if (typeof item === 'number') {
                    // It's an existing file ID
                    formData.append('BFiles', String(item));
                } else {
                    // It's a new File object
                    formData.append('BFiles', item);
                }
            });
        }

        // Add sell invoice files (both file IDs and new files)
        if (dto.sellInvoiceFiles && dto.sellInvoiceFiles.length > 0) {
            dto.sellInvoiceFiles.forEach(item => {
                if (typeof item === 'number') {
                    // It's an existing file ID
                    formData.append('SFiles', String(item));
                } else {
                    // It's a new File object
                    formData.append('SFiles', item);
                }
            });
        }

        // Add removed buy file IDs
        if (dto.removedBuyFileIds && dto.removedBuyFileIds.length > 0) {
            dto.removedBuyFileIds.forEach(id => {
                formData.append('RemovedBuyFileIds', String(id));
            });
        }

        // Add removed sell file IDs
        if (dto.removedSellFileIds && dto.removedSellFileIds.length > 0) {
            dto.removedSellFileIds.forEach(id => {
                formData.append('RemovedSellFileIds', String(id));
            });
        }

        dto.assignedByUserId?.forEach(id => formData.append('UserIds', String(id)));

        return formData;
    }

    async assignEmployee(id: number | string, payload: any) {
        await this.repo.assignEmployee(id, payload);
    }

    async setStageAssignee(orderId: number | string, stageId: number | string, userId: number | null) {
        await this.repo.setStageAssignee(orderId, stageId, userId);
    }

    async create(dto: OrderServiceDTO) {
        const formData = this.buildFormData(dto);
        await this.repo.createOrder(formData);
    }
    async update(id: number | string, dto: OrderServiceDTO) {
        const formData = this.buildFormData(dto);
        await this.repo.updateOrder(id, formData);
    }

    // Payment FormData builders
    buildPaymentFormData(dto: PaymentCreateDTO) {
        const formData = new FormData();
        formData.append('Amount', String(dto.amount));
        formData.append('PaymentType', dto.paymentType);
        if (dto.description) {
            formData.append('Description', dto.description);
        }
        if (dto.files) {
            dto.files.forEach(file => {
                formData.append('Files', file);
            });
        }
        return formData;
    }

    buildUpdatePaymentFormData(dto: PaymentUpdateDTO) {
        const formData = new FormData();
        formData.append('Amount', String(dto.amount));
        formData.append('PaymentType', dto.paymentType);
        if (dto.description) {
            formData.append('Description', dto.description);
        }

        // Add both file IDs and new files to Files field
        if (dto.files && dto.files.length > 0) {
            dto.files.forEach(item => {
                if (typeof item === 'number') {
                    // It's an existing file ID
                    formData.append('Files', String(item));
                } else {
                    // It's a new File object
                    formData.append('Files', item);
                }
            });
        }

        // Add removed file IDs
        if (dto.removedFileIds && dto.removedFileIds.length > 0) {
            dto.removedFileIds.forEach(id => {
                formData.append('RemovedFileIds', String(id));
            });
        }

        return formData;
    }

    // Client Payments
    async getClientPayments(orderId: string | number): Promise<PaymentsListResponse> {
        return this.repo.fetchClientPayments(orderId);
    }

    async getClientPaymentDetail(orderId: string | number, paymentId: string | number): Promise<PaymentDetailResponse> {
        return this.repo.fetchClientPaymentDetail(orderId, paymentId);
    }

    async createClientPayment(orderId: string | number, dto: PaymentCreateDTO) {
        const formData = this.buildPaymentFormData(dto);
        await this.repo.createClientPayment(orderId, formData);
    }

    async updateClientPayment(orderId: string | number, paymentId: string | number, dto: PaymentUpdateDTO) {
        const formData = this.buildUpdatePaymentFormData(dto);
        await this.repo.updateClientPayment(orderId, paymentId, formData);
    }

    async deleteClientPayment(orderId: string | number, paymentId: string | number) {
        await this.repo.deleteClientPayment(orderId, paymentId);
    }

    // Provider Payments
    async getProviderPayments(orderId: string | number): Promise<PaymentsListResponse> {
        return this.repo.fetchProviderPayments(orderId);
    }

    async getProviderPaymentDetail(orderId: string | number, paymentId: string | number): Promise<PaymentDetailResponse> {
        return this.repo.fetchProviderPaymentDetail(orderId, paymentId);
    }

    async createProviderPayment(orderId: string | number, dto: PaymentCreateDTO) {
        const formData = this.buildPaymentFormData(dto);
        await this.repo.createProviderPayment(orderId, formData);
    }

    async updateProviderPayment(orderId: string | number, paymentId: string | number, dto: PaymentUpdateDTO) {
        const formData = this.buildUpdatePaymentFormData(dto);
        await this.repo.updateProviderPayment(orderId, paymentId, formData);
    }

    async deleteProviderPayment(orderId: string | number, paymentId: string | number) {
        await this.repo.deleteProviderPayment(orderId, paymentId);
    }

    // Extra Providers
    async addExtraProvider(orderId: string | number, dto: ExtraProviderCreateDTO) {
        return this.repo.addExtraProvider(orderId, dto);
    }

    async removeExtraProvider(orderId: string | number, epId: string | number) {
        await this.repo.removeExtraProvider(orderId, epId);
    }

    async getExtraProviderPaymentSummary(orderId: string | number, epId: string | number): Promise<ExtraProviderPaymentSummaryResponse> {
        return this.repo.fetchExtraProviderPaymentSummary(orderId, epId);
    }

    async getExtraProviderPaymentDetail(orderId: string | number, paymentId: string | number): Promise<ExtraProviderPaymentDetailResponse> {
        return this.repo.fetchExtraProviderPaymentDetail(orderId, paymentId);
    }

    async createExtraProviderPayment(orderId: string | number, epId: string | number, dto: PaymentCreateDTO) {
        const formData = this.buildPaymentFormData(dto);
        await this.repo.createExtraProviderPayment(orderId, epId, formData);
    }

    async updateExtraProviderPayment(orderId: string | number, paymentId: string | number, dto: PaymentUpdateDTO) {
        const formData = this.buildUpdatePaymentFormData(dto);
        await this.repo.updateExtraProviderPayment(orderId, paymentId, formData);
    }

    async deleteExtraProviderPayment(orderId: string | number, paymentId: string | number) {
        await this.repo.deleteExtraProviderPayment(orderId, paymentId);
    }

    // Order Actions
    async completeOrder(orderId: string | number, formData: FormData) {
        await this.repo.completeOrder(orderId, formData);
    }

    async cancelOrder(orderId: string | number, formData: FormData) {
        await this.repo.cancelOrder(orderId, formData);
    }

    async suspendOrder(orderId: string | number, formData: FormData) {
        await this.repo.suspendOrder(orderId, formData);
    }

    async unsuspendOrder(orderId: string | number, formData: FormData) {
        await this.repo.unsuspendOrder(orderId, formData);
    }

    // Notes
    buildNoteFormData(dto: { actionDate: string; title: string; description?: string; files?: File[] }) {
        const formData = new FormData();
        formData.append('ActionDate', dto.actionDate);
        formData.append('Title', dto.title);
        if (dto.description) {
            formData.append('Description', dto.description);
        }
        if (dto.files) {
            dto.files.forEach(file => {
                formData.append('Files', file);
            });
        }
        return formData;
    }

    async createNote(orderId: string | number, dto: { actionDate: string; title: string; description?: string; files?: File[] }) {
        const formData = this.buildNoteFormData(dto);
        await this.repo.createNote(orderId, formData);
    }

    async getNoteDetail(orderId: string | number, noteId: string | number): Promise<NoteDetailResponse> {
        return this.repo.fetchNoteDetail(orderId, noteId);
    }

    buildUpdateNoteFormData(dto: { actionDate: string; title: string; description?: string; files?: File[]; removedFileIds?: number[] }) {
        const formData = new FormData();
        formData.append('ActionDate', dto.actionDate);
        formData.append('Title', dto.title);
        if (dto.description) {
            formData.append('Description', dto.description);
        }
        if (dto.files) {
            dto.files.forEach(file => {
                formData.append('Files', file);
            });
        }
        if (dto.removedFileIds && dto.removedFileIds.length > 0) {
            dto.removedFileIds.forEach(id => {
                formData.append('RemovedFileIds', String(id));
            });
        }
        return formData;
    }

    async updateNote(orderId: string | number, noteId: string | number, dto: { actionDate: string; title: string; description?: string; files?: File[]; removedFileIds?: number[] }) {
        const formData = this.buildUpdateNoteFormData(dto);
        await this.repo.updateNote(orderId, noteId, formData);
    }

    // Step Actions
    buildStepCompleteFormData(dto: { orderId: number | string; stepId: number | string; actionDate: string; description: string; files?: File[] }) {
        const formData = new FormData();
        formData.append('OrderId', String(dto.orderId));
        formData.append('StepId', String(dto.stepId));
        formData.append('ActionDate', dto.actionDate);
        formData.append('Description', dto.description);
        if (dto.files) {
            dto.files.forEach(file => {
                formData.append('Files', file);
            });
        }
        return formData;
    }

    async setStepComplete(dto: { orderId: number | string; stepId: number | string; actionDate: string; description: string; files?: File[] }) {
        const formData = this.buildStepCompleteFormData(dto);
        await this.repo.setStepComplete(formData);
    }

    // Change Step
    buildSetStepFormData(dto: { orderId: number | string; stepId: number | string; actionDate: string; description: string; files?: File[] }) {
        const formData = new FormData();
        formData.append('OrderId', String(dto.orderId));
        formData.append('StepId', String(dto.stepId));
        formData.append('ActionDate', dto.actionDate);
        formData.append('Description', dto.description);
        if (dto.files) {
            dto.files.forEach(file => {
                formData.append('Files', file);
            });
        }
        return formData;
    }

    async setStep(dto: { orderId: number | string; stepId: number | string; actionDate: string; description: string; files?: File[] }) {
        const formData = this.buildSetStepFormData(dto);
        await this.repo.setStep(formData);
    }

    async getSteps(): Promise<StepOption[]> {
        return this.repo.getSteps();
    }

    async getStepOptions(): Promise<{ label: string; value: number }[]> {
        const steps = await this.repo.getSteps();
        return steps
            .filter(step => step.isActive)
            .map(step => ({
                label: step.name,
                value: step.id,
            }));
    }

    async getSummary(): Promise<OrdersSummaryResponse> {
        return this.repo.fetchSummary();
    }
}
