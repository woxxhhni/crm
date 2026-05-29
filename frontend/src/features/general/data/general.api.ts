import { HttpClientInstance } from '@/services/http-client.service';
import { GeneralEndpoints } from '@/api/general/endpoints';
import { CurrentUser } from '@/api/general/types';

export class GeneralRepository {
  async fetchCurrentUser(): Promise<CurrentUser> {
    const { data } = await HttpClientInstance.get<CurrentUser>(GeneralEndpoints.me);
    return data;
  }
}
