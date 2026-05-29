import { injectable } from 'tsyringe';
import { GeneralRepository } from '../data/general.api';
import type { CurrentUser } from '@/api/general/types';

@injectable()
export class GeneralService {
  constructor(private repo: GeneralRepository) {}

  async getCurrentUser(): Promise<CurrentUser> {
    return this.repo.fetchCurrentUser();
  }
}
