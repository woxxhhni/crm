export interface ApiValidationError {
    propertyName: string;
    errorMessage: string;
}

export interface ApiErrorResponse {
    type?: string;
    title?: string;
    status?: number;
    detail?: string;
    errors?: ApiValidationError[];
}

export function parseApiError(error: unknown): ApiErrorResponse {
    if (error && typeof error === 'object' && 'response' in error) {
        const axiosError = error as { response?: { data?: ApiErrorResponse; status?: number } };
        if (axiosError.response?.data) {
            return {
                ...axiosError.response.data,
                status: axiosError.response.status,
            };
        }
    }
    return {
        title: 'Error',
        detail: 'An unexpected error occurred',
        status: 500,
    };
}

export function formatApiErrorMessage(error: unknown): string {
    const apiError = parseApiError(error);

    // If there are validation errors, format them
    if (apiError.errors && apiError.errors.length > 0) {
        return apiError.errors.map(e => e.errorMessage).join('\n');
    }

    // Return detail or title
    return apiError.detail || apiError.title || 'An unexpected error occurred';
}

export function getValidationErrors(error: unknown): Record<string, string> {
    const apiError = parseApiError(error);
    const validationErrors: Record<string, string> = {};

    if (apiError.errors) {
        apiError.errors.forEach(err => {
            const fieldName = err.propertyName.charAt(0).toLowerCase() + err.propertyName.slice(1);
            validationErrors[fieldName] = err.errorMessage;
        });
    }

    return validationErrors;
}
