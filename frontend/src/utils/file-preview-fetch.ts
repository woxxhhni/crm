import Cookies from 'js-cookie';
import { ENV } from '@/env';
import { JWT_STORAGE_KEY } from '@/features/auth/sign-in/context/jwt/constant';

function apiUrl(path: string) {
  const baseUrl = ENV.apiBaseUrl.replace(/\/$/, '');
  return `${baseUrl}${path}`;
}

async function fetchArrayBuffer(url: string, headers?: HeadersInit) {
  const response = await fetch(url, { headers });
  if (!response.ok) throw new Error(`Failed to fetch file: ${response.status}`);
  return response.arrayBuffer();
}

export async function fetchPreviewArrayBuffer(url: string, fileId?: number | string) {
  if (fileId) {
    const token = Cookies.get(JWT_STORAGE_KEY);
    try {
      return await fetchArrayBuffer(apiUrl(`/api/v1/Files/${fileId}/content`), token ? { Authorization: `Bearer ${token}` } : undefined);
    } catch {
      // Fall back to the original file URL for blob/object URLs and public presigned links.
    }
  }

  return fetchArrayBuffer(url);
}
