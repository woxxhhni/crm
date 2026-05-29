export const EmployeesEndpoints = {
    root: '/api/v1/Employees',
    byId: (id: string | number) => `/api/v1/Employees/${id}`,
    resetPassword: (id: string | number) => `/api/v1/Employees/${id}/reset-password`,
} as const;
