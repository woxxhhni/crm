import { HttpClientInstance } from '@/services/http-client.service';
import { AuthEndpoints } from '@/api/auth/endpoints';
import { LoginRequest, LoginResponse } from '@/api/auth/types';

export class AuthRepository {
    async login(payload: LoginRequest, recaptchaToken?: string): Promise<LoginResponse> {
        const headers: Record<string, string> = {};
        if (recaptchaToken) {
            headers['X-Recaptcha-Token'] = recaptchaToken;
        }

        const { data } = await HttpClientInstance.post<LoginResponse>(
            AuthEndpoints.LOGIN,
            payload,
            { headers }
        );
        return data;
    }
}
