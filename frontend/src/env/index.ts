declare global {
    interface Window {
        __ENV?: {
            NEXT_PUBLIC_API_BASE_URL?: string;
            NEXT_PUBLIC_TIMEZONE?: string;
            NEXT_PUBLIC_RECAPTCHA_SITE_KEY?: string;
        };
    }
}

export const ENV = {
    get apiBaseUrl(): string {
        // Server-side: use process.env
        if (typeof window === 'undefined') {
            return process.env.NEXT_PUBLIC_API_BASE_URL || '';
        }
        // Client-side: Docker runtime config (from entrypoint.sh) OR Next.js build-time env
        return window.__ENV?.NEXT_PUBLIC_API_BASE_URL || process.env.NEXT_PUBLIC_API_BASE_URL || '';
    },

    get timezone(): string {
        // Server-side: use process.env
        if (typeof window === 'undefined') {
            return process.env.NEXT_PUBLIC_TIMEZONE || 'America/Toronto';
        }
        // Client-side: Docker runtime config (from entrypoint.sh) OR Next.js build-time env
        return window.__ENV?.NEXT_PUBLIC_TIMEZONE || process.env.NEXT_PUBLIC_TIMEZONE || 'America/Toronto';
    },

    get recaptchaSiteKey(): string {
        // Server-side: use process.env
        if (typeof window === 'undefined') {
            return process.env.NEXT_PUBLIC_RECAPTCHA_SITE_KEY || '';
        }
        // Client-side: Docker runtime config (from entrypoint.sh) OR Next.js build-time env
        return window.__ENV?.NEXT_PUBLIC_RECAPTCHA_SITE_KEY || process.env.NEXT_PUBLIC_RECAPTCHA_SITE_KEY || '';
    },
};
