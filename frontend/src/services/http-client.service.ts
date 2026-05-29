import axios, { AxiosInstance, AxiosRequestConfig } from 'axios';
import Cookies from 'js-cookie';
import { ENV } from '@/env';
import { JWT_STORAGE_KEY } from '@/features/auth/sign-in/context/jwt/constant';

// Event name for 403 errors
export const FORBIDDEN_ERROR_EVENT = 'app:forbidden-error';
export const REFETCH_USER_EVENT = 'app:refetch-user';

export class HttpClient {
    private client: AxiosInstance;

    constructor() {
        this.client = axios.create({
            baseURL: ENV.apiBaseUrl,
            headers: { 'Content-Type': 'application/json' },
        });
        this.setupInterceptors();
    }

    private dispatchForbiddenError(message: string) {
        if (typeof window !== 'undefined') {
            window.dispatchEvent(new CustomEvent(FORBIDDEN_ERROR_EVENT, { detail: message }));
            window.dispatchEvent(new CustomEvent(REFETCH_USER_EVENT));
        }
    }

    private setupInterceptors() {
        this.client.interceptors.request.use((config) => {
            const token = this.getAuthToken();
            if (token) config.headers.Authorization = `Bearer ${token}`;
            return config;
        });

        this.client.interceptors.response.use(
            (response) => response,
            (error) => {
                if (error.response?.status === 401) {
                    // Don't redirect if this is the login endpoint itself
                    const isLoginEndpoint = error.config?.url?.includes('/Auth/login');

                    if (!isLoginEndpoint && typeof window !== 'undefined') {
                        // Unauthorized - clear session and redirect to login
                        Cookies.remove(JWT_STORAGE_KEY);
                        delete this.client.defaults.headers.common.Authorization;
                        window.location.href = '/auth/sign-in';
                    }
                } else if (error.response?.status === 403) {
                    this.dispatchForbiddenError(error.response?.data?.message || 'You do not have permission to perform this action');
                }
                return Promise.reject(error);
            }
        );
    }

    private getAuthToken() {
        if (typeof window === 'undefined') return null;
        return Cookies.get(JWT_STORAGE_KEY);
    }

    async get<T>(
        url: string,
        params?: Record<string, any>,
        config?: AxiosRequestConfig
    ) {
        const finalConfig: AxiosRequestConfig = {
            ...config,
            params,
        };
        return this.client.get<T>(url, finalConfig);
    }


    async post<T>(url: string, data?: any, config?: AxiosRequestConfig) {
        return this.client.post<T>(url, data, config);
    }

    async put<T>(url: string, data?: any, config?: AxiosRequestConfig) {
        return this.client.put<T>(url, data, config);
    }

    async patch<T>(url: string, data?: any, config?: AxiosRequestConfig) {
        return this.client.patch<T>(url, data, config);
    }

    async delete<T>(url: string, config?: AxiosRequestConfig) {
        return this.client.delete<T>(url, config);
    }

    setAuthToken(token: string | null) {
        if (token)
            this.client.defaults.headers.common.Authorization = `Bearer ${token}`;
        else
            delete this.client.defaults.headers.common.Authorization;
    }
}

export const HttpClientInstance = new HttpClient();
