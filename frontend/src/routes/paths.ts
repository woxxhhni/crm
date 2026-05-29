// ----------------------------------------------------------------------

const ROOTS = {
    AUTH: '/auth',
    DASHBOARD: '/panel',
};

// ----------------------------------------------------------------------

export const paths = {
    auth: {
        signIn: `${ROOTS.AUTH}/sign-in`,
    },
    panel: {
        root: ROOTS.DASHBOARD,
        dashboard: `${ROOTS.DASHBOARD}/dashboard`,
        clients: `${ROOTS.DASHBOARD}/clients`,
        providers: `${ROOTS.DASHBOARD}/providers`,
        employees: `${ROOTS.DASHBOARD}/employees`,
        employeeDetails: (id: number | string) => `${ROOTS.DASHBOARD}/employees/details/${id}`,
        orders: `${ROOTS.DASHBOARD}/orders`,
        orderDetails: (id: number | string) => `${ROOTS.DASHBOARD}/orders/details/${id}`,
        clientPaymentDetail: (orderId: number | string, paymentId: number | string) => `${ROOTS.DASHBOARD}/orders/${orderId}/client-payments/${paymentId}`,
        providerPaymentDetail: (orderId: number | string, paymentId: number | string) => `${ROOTS.DASHBOARD}/orders/${orderId}/provider-payments/${paymentId}`,
        extraProviderDetail: (orderId: number | string, epId: number | string) => `${ROOTS.DASHBOARD}/orders/${orderId}/extra-providers/${epId}`,
        orderLogDetail: (orderId: number | string, logId: number | string) => `${ROOTS.DASHBOARD}/orders/${orderId}/logs/${logId}`,
    },
};
