import { HttpClientInstance } from '@/services/http-client.service';
import { OrdersEndpoints } from '@/api/orders/endpoints';


import { ProviderListResponse, ProviderRecord } from "@/api/providers/types";
import { ProvidersEndpoints } from "@/api/providers/endpoints";
import { commonEndpoints } from "@/api/common/endpoints";
import { ClientListResponse } from "@/api/clients/types";
import { ClientEndpoints } from "@/api/clients/endpoints";
import { OrderRecord, OrdersListResponse, PaymentsListResponse, PaymentDetailResponse, StepOption, NoteDetailResponse, OrdersSummaryResponse, ExtraProviderPaymentSummaryResponse, ExtraProviderPaymentDetailResponse } from "@/api/orders/types";

export class OrdersRepository {
    async deleteOrder(id: number | string): Promise<void> {
        await HttpClientInstance.delete(OrdersEndpoints.deleteOrder(id));
    }

    async deleteOrderUniqueNumber(id: number | string, label: string): Promise<void> {
        await HttpClientInstance.delete(
            OrdersEndpoints.delete(id, label)
        );
    }
    async deleteOrderEmployee(orderId: number | string, employeeId: number | string): Promise<void> {
        await HttpClientInstance.delete(
            OrdersEndpoints.deleteOrderEmployee(orderId, employeeId)
        );
    }
    async createOrder(payload: FormData): Promise<void> {
        await HttpClientInstance.post(OrdersEndpoints.root, payload, {
            headers: { 'Content-Type': 'multipart/form-data' },
        });
    }

    async updateOrder(id: number | string, payload: FormData) {
        await HttpClientInstance.put(OrdersEndpoints.byId(id), payload, {
            headers: { 'Content-Type': 'multipart/form-data' },
        });
    }

    async fetchOrders(params?: { page?: number; pageSize?: number; filters?: Record<string, any> }): Promise<OrdersListResponse> {
        const { page, pageSize, filters } = params || {};

        const queryParams = new URLSearchParams();
        if (page) queryParams.append('page', String(page));
        if (pageSize) queryParams.append('pageSize', String(pageSize));

        Object.entries(filters || {}).forEach(([key, value]) => {
            if (Array.isArray(value)) {
                value.forEach(v => queryParams.append(key, String(v))); // ⬅ بدون []
            } else if (value !== undefined && value !== null && value !== '') {
                queryParams.append(key, String(value));
            }
        });

        const { data } = await HttpClientInstance.get<OrdersListResponse>(
            `${OrdersEndpoints.root}?${queryParams.toString()}`
        );

        return data;
    }

    async assignEmployee(id: number | string, payload: any): Promise<any> {
        const { data } = await HttpClientInstance.post<any>(OrdersEndpoints.ordersEmployee(id), payload);
        return data;
    }

    async setStageAssignee(orderId: number | string, stageId: number | string, userId: number | null): Promise<void> {
        await HttpClientInstance.put(OrdersEndpoints.stageAssignee(orderId, stageId), { userId });
    }

    async fetchOrderDetails(id: number | string): Promise<OrderRecord> {
        const { data } = await HttpClientInstance.get<OrderRecord>(OrdersEndpoints.byId(id));
        return data;
    }

    private readonly DEFAULT_PAGE_SIZE = 9999;

    async fetchProviders(): Promise<ProviderListResponse> {
        const queryParams = new URLSearchParams();
        queryParams.append('pageSize', String(this.DEFAULT_PAGE_SIZE));
        const { data } = await HttpClientInstance.get<ProviderListResponse>(`${ProvidersEndpoints.root}?${queryParams.toString()}`);
        return data;
    }
    async fetchClients(): Promise<ClientListResponse> {
        const queryParams = new URLSearchParams();
        queryParams.append('pageSize', String(this.DEFAULT_PAGE_SIZE));
        const { data } = await HttpClientInstance.get<ClientListResponse>(`${ClientEndpoints.root}?${queryParams.toString()}`);
        return data;
    }
    async fetchStatuses(): Promise<any> {
        const { data } = await HttpClientInstance.get<any>(commonEndpoints.statuses);
        return data;
    }
    async fetchCurrencies(): Promise<any> {
        const { data } = await HttpClientInstance.get<any>(commonEndpoints.currencies);
        return data;
    }
    async fetchEmployees(): Promise<any> {
        const queryParams = new URLSearchParams();
        queryParams.append('pageSize', String(this.DEFAULT_PAGE_SIZE));
        const { data } = await HttpClientInstance.get<any>(`${commonEndpoints.employees}?${queryParams.toString()}`);
        return data;
    }

    // Client Payments
    async fetchClientPayments(orderId: string | number): Promise<PaymentsListResponse> {
        const { data } = await HttpClientInstance.get<PaymentsListResponse>(OrdersEndpoints.clientPayments(orderId));
        return data;
    }

    async fetchClientPaymentDetail(orderId: string | number, paymentId: string | number): Promise<PaymentDetailResponse> {
        const { data } = await HttpClientInstance.get<PaymentDetailResponse>(OrdersEndpoints.clientPaymentById(orderId, paymentId));
        return data;
    }

    async createClientPayment(orderId: string | number, payload: FormData): Promise<void> {
        await HttpClientInstance.post(OrdersEndpoints.clientPayments(orderId), payload, {
            headers: { 'Content-Type': 'multipart/form-data' },
        });
    }

    async updateClientPayment(orderId: string | number, paymentId: string | number, payload: FormData): Promise<void> {
        await HttpClientInstance.put(OrdersEndpoints.clientPaymentById(orderId, paymentId), payload, {
            headers: { 'Content-Type': 'multipart/form-data' },
        });
    }

    async deleteClientPayment(orderId: string | number, paymentId: string | number): Promise<void> {
        await HttpClientInstance.delete(OrdersEndpoints.clientPaymentById(orderId, paymentId));
    }

    // Provider Payments
    async fetchProviderPayments(orderId: string | number): Promise<PaymentsListResponse> {
        const { data } = await HttpClientInstance.get<PaymentsListResponse>(OrdersEndpoints.providerPayments(orderId));
        return data;
    }

    async fetchProviderPaymentDetail(orderId: string | number, paymentId: string | number): Promise<PaymentDetailResponse> {
        const { data } = await HttpClientInstance.get<PaymentDetailResponse>(OrdersEndpoints.providerPaymentById(orderId, paymentId));
        return data;
    }

    async createProviderPayment(orderId: string | number, payload: FormData): Promise<void> {
        await HttpClientInstance.post(OrdersEndpoints.providerPayments(orderId), payload, {
            headers: { 'Content-Type': 'multipart/form-data' },
        });
    }

    async updateProviderPayment(orderId: string | number, paymentId: string | number, payload: FormData): Promise<void> {
        await HttpClientInstance.put(OrdersEndpoints.providerPaymentById(orderId, paymentId), payload, {
            headers: { 'Content-Type': 'multipart/form-data' },
        });
    }

    async deleteProviderPayment(orderId: string | number, paymentId: string | number): Promise<void> {
        await HttpClientInstance.delete(OrdersEndpoints.providerPaymentById(orderId, paymentId));
    }

    // Order Actions
    async completeOrder(orderId: string | number, payload: FormData): Promise<void> {
        await HttpClientInstance.patch(OrdersEndpoints.complete(orderId), payload, {
            headers: { 'Content-Type': 'multipart/form-data' },
        });
    }

    async cancelOrder(orderId: string | number, payload: FormData): Promise<void> {
        await HttpClientInstance.patch(OrdersEndpoints.cancel(orderId), payload, {
            headers: { 'Content-Type': 'multipart/form-data' },
        });
    }

    async suspendOrder(orderId: string | number, payload: FormData): Promise<void> {
        await HttpClientInstance.patch(OrdersEndpoints.suspend(orderId), payload, {
            headers: { 'Content-Type': 'multipart/form-data' },
        });
    }

    async unsuspendOrder(orderId: string | number, payload: FormData): Promise<void> {
        await HttpClientInstance.patch(OrdersEndpoints.unsuspend(orderId), payload, {
            headers: { 'Content-Type': 'multipart/form-data' },
        });
    }

    async reopenOrder(orderId: string | number, payload: FormData): Promise<void> {
        await HttpClientInstance.patch(OrdersEndpoints.reopen(orderId), payload, {
            headers: { 'Content-Type': 'multipart/form-data' },
        });
    }

    // Notes
    async createNote(orderId: string | number, payload: FormData): Promise<void> {
        await HttpClientInstance.post(OrdersEndpoints.notes(orderId), payload, {
            headers: { 'Content-Type': 'multipart/form-data' },
        });
    }

    async fetchNoteDetail(orderId: string | number, noteId: string | number): Promise<NoteDetailResponse> {
        const { data } = await HttpClientInstance.get<NoteDetailResponse>(OrdersEndpoints.noteById(orderId, noteId));
        return data;
    }

    async updateNote(orderId: string | number, noteId: string | number, payload: FormData): Promise<void> {
        await HttpClientInstance.put(OrdersEndpoints.noteById(orderId, noteId), payload, {
            headers: { 'Content-Type': 'multipart/form-data' },
        });
    }

    // Step Actions
    async setStepComplete(payload: FormData): Promise<void> {
        await HttpClientInstance.patch(OrdersEndpoints.setStepComplete, payload, {
            headers: { 'Content-Type': 'multipart/form-data' },
        });
    }

    async setStep(payload: FormData): Promise<void> {
        await HttpClientInstance.patch(OrdersEndpoints.setStep, payload, {
            headers: { 'Content-Type': 'multipart/form-data' },
        });
    }

    async getSteps(): Promise<StepOption[]> {
        const { data } = await HttpClientInstance.get<StepOption[]>(OrdersEndpoints.getSteps);
        return data;
    }

    async fetchSummary(): Promise<OrdersSummaryResponse> {
        const { data } = await HttpClientInstance.get<OrdersSummaryResponse>(OrdersEndpoints.summary);
        return data;
    }

    // Extra Providers
    async addExtraProvider(orderId: string | number, payload: any): Promise<number> {
        const { data } = await HttpClientInstance.post<number>(OrdersEndpoints.extraProviders(orderId), payload);
        return data;
    }

    async removeExtraProvider(orderId: string | number, epId: string | number): Promise<void> {
        await HttpClientInstance.delete(OrdersEndpoints.extraProviderById(orderId, epId));
    }

    async fetchExtraProviderPaymentSummary(orderId: string | number, epId: string | number): Promise<ExtraProviderPaymentSummaryResponse> {
        const { data } = await HttpClientInstance.get<ExtraProviderPaymentSummaryResponse>(OrdersEndpoints.extraProviderPayments(orderId, epId));
        return data;
    }

    async fetchExtraProviderPaymentDetail(orderId: string | number, paymentId: string | number): Promise<ExtraProviderPaymentDetailResponse> {
        const { data } = await HttpClientInstance.get<ExtraProviderPaymentDetailResponse>(OrdersEndpoints.extraProviderPaymentById(orderId, paymentId));
        return data;
    }

    async createExtraProviderPayment(orderId: string | number, epId: string | number, payload: FormData): Promise<void> {
        await HttpClientInstance.post(OrdersEndpoints.extraProviderPayments(orderId, epId), payload, {
            headers: { 'Content-Type': 'multipart/form-data' },
        });
    }

    async updateExtraProviderPayment(orderId: string | number, paymentId: string | number, payload: FormData): Promise<void> {
        await HttpClientInstance.put(OrdersEndpoints.extraProviderPaymentById(orderId, paymentId), payload, {
            headers: { 'Content-Type': 'multipart/form-data' },
        });
    }

    async deleteExtraProviderPayment(orderId: string | number, paymentId: string | number): Promise<void> {
        await HttpClientInstance.delete(OrdersEndpoints.extraProviderPaymentById(orderId, paymentId));
    }
}
