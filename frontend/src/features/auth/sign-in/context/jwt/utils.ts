import Cookies from 'js-cookie';
import { JWT_STORAGE_KEY } from './constant';
import { HttpClientInstance } from '@/services/http-client.service';

// Cookie options
const COOKIE_OPTIONS: Cookies.CookieAttributes = {
  expires: 3, // 3 days
  secure: process.env.NODE_ENV === 'production',
  sameSite: 'strict',
};

// ----------------------------------------------------------------------

export function jwtDecode(token: string) {
  try {
    if (!token) return null;

    const parts = token.split('.');
    if (parts.length < 2) {
      throw new Error('Invalid token!');
    }

    const base64Url = parts[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const decoded = JSON.parse(atob(base64));

    return decoded;
  } catch (error) {
    console.error('Error decoding token:', error);
    throw error;
  }
}

// ----------------------------------------------------------------------

export function isValidToken(accessToken: string) {
  if (!accessToken) {
    return false;
  }

  try {
    const decoded = jwtDecode(accessToken);

    if (!decoded || !('exp' in decoded)) {
      return false;
    }

    const currentTime = Date.now() / 1000;

    return decoded.exp > currentTime;
  } catch (error) {
    console.error('Error during token validation:', error);
    return false;
  }
}

// ----------------------------------------------------------------------

export function tokenExpired(exp: number) {
  const currentTime = Date.now();
  const timeLeft = exp * 1000 - currentTime;

  setTimeout(() => {
    try {
      alert('Token expired!');
      Cookies.remove(JWT_STORAGE_KEY);
      window.location.href = '/auth/sign-in';
    } catch (error) {
      console.error('Error during token expiration:', error);
      throw error;
    }
  }, timeLeft);
}

// ----------------------------------------------------------------------

export function getAccessToken(): string | undefined {
  return Cookies.get(JWT_STORAGE_KEY);
}

// ----------------------------------------------------------------------

export async function setSession(accessToken: string | null) {
  try {
    if (accessToken) {
      Cookies.set(JWT_STORAGE_KEY, accessToken, COOKIE_OPTIONS);

      HttpClientInstance.setAuthToken(accessToken);

      const decodedToken = jwtDecode(accessToken);

      if (decodedToken && 'exp' in decodedToken) {
        tokenExpired(decodedToken.exp);
      } else {
        throw new Error('Invalid access token!');
      }
    } else {
      Cookies.remove(JWT_STORAGE_KEY);
      HttpClientInstance.setAuthToken(null);
    }
  } catch (error) {
    console.error('Error during set session:', error);
    throw error;
  }
}
