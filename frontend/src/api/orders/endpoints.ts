export const OrdersEndpoints = {
    root: '/api/v1/Orders',
    summary: '/api/v1/Orders/summary',
    byId: (id: string | number) => `/api/v1/Orders/${id}`,
    deleteOrder: (id: string | number) => `/api/v1/Orders/${id}`,
    ordersEmployee: (id: string | number) => `/api/v1/Orders/${id}/employees`,
    stageAssignee: (orderId: string | number, stageId: string | number) => `/api/v1/Orders/${orderId}/stages/${stageId}/assignee`,
    delete: (id: string | number, label: string) => `/api/v1/Orders/${id}/unique-numbers/${label}`,
    deleteOrderEmployee: (orderId: number | string, employeeId: number | string) => `/api/v1/Orders/${orderId}/employees/${employeeId}`,
    // Order Actions
    complete: (orderId: string | number) => `/api/v1/Orders/${orderId}/complete`,
    cancel: (orderId: string | number) => `/api/v1/Orders/${orderId}/cancel`,
    suspend: (orderId: string | number) => `/api/v1/Orders/${orderId}/suspend`,
    unsuspend: (orderId: string | number) => `/api/v1/Orders/${orderId}/unsuspend`,
    reopen: (orderId: string | number) => `/api/v1/Orders/${orderId}/reopen`,
    // Client Payments
    clientPayments: (orderId: string | number) => `/api/v1/Orders/${orderId}/client-payments`,
    clientPaymentById: (orderId: string | number, paymentId: string | number) => `/api/v1/Orders/${orderId}/client-payments/${paymentId}`,
    // Provider Payments
    providerPayments: (orderId: string | number) => `/api/v1/Orders/${orderId}/provider-payments`,
    providerPaymentById: (orderId: string | number, paymentId: string | number) => `/api/v1/Orders/${orderId}/provider-payments/${paymentId}`,
    // Notes
    notes: (orderId: string | number) => `/api/v1/Orders/${orderId}/notes`,
    noteById: (orderId: string | number, noteId: string | number) => `/api/v1/Orders/${orderId}/notes/${noteId}`,
    // Step Actions
    setStepComplete: '/api/v1/Orders/set-step-complete',
    setStep: '/api/v1/Orders/set-step',
    // Extra Providers
    extraProviders: (orderId: string | number) => `/api/v1/Orders/${orderId}/extra-providers`,
    extraProviderById: (orderId: string | number, epId: string | number) => `/api/v1/Orders/${orderId}/extra-providers/${epId}`,
    extraProviderPayments: (orderId: string | number, epId: string | number) => `/api/v1/Orders/${orderId}/extra-providers/${epId}/payments`,
    extraProviderPaymentById: (orderId: string | number, paymentId: string | number) => `/api/v1/Orders/${orderId}/extra-providers/payments/${paymentId}`,
    // Steps
    getSteps: '/api/v1/Steps/get-list',
} as const;
