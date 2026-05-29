import { injectable } from 'tsyringe';
import { AuthRepository } from '../data/auth.api';

@injectable()
export class AuthService {
    constructor(private readonly authRepo: AuthRepository) { }

    async login(email: string, password: string, recaptchaToken?: string) {
        const response = await this.authRepo.login({ email, password }, recaptchaToken);
        return response;
    }
}
