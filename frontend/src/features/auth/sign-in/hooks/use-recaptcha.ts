import { useCallback, useEffect, useState } from 'react';
import { ENV } from '@/env';

export const useRecaptcha = () => {
    const [isReady, setIsReady] = useState(false);

    useEffect(() => {
        // Skip loading reCAPTCHA if no site key is configured
        if (!ENV.recaptchaSiteKey) return;

        const scriptId = 'google-recaptcha-script';
        if (document.getElementById(scriptId)) {
            setIsReady(true);
            return;
        }

        const script = document.createElement('script');
        script.id = scriptId;
        script.src = `https://www.google.com/recaptcha/api.js?render=${ENV.recaptchaSiteKey}`;
        script.async = true;

        script.onload = () => {
            setIsReady(true);
        };

        document.body.appendChild(script);

        return () => {
            // Keep the script and badge to prevent 'Invalid reCAPTCHA client id' errors
            // caused by React Strict Mode unmounting and remounting.
        };
    }, []);

    const executeRecaptcha = useCallback(async (action: string): Promise<string | null> => {
        try {
            if (typeof window !== 'undefined' && 'grecaptcha' in window) {
                const grecaptcha = (window as any).grecaptcha;
                return await new Promise<string | null>((resolve) => {
                    grecaptcha.ready(() => {
                        try {
                            grecaptcha.execute(ENV.recaptchaSiteKey, { action })
                                .then((token: string) => resolve(token))
                                .catch((err: any) => {
                                    console.error('Recaptcha execute promise error', err);
                                    resolve(null);
                                });
                        } catch (e) {
                            console.error('Recaptcha execute sync error', e);
                            resolve(null);
                        }
                    });
                });
            }
        } catch (error) {
            console.error('Recaptcha execution failed', error);
        }
        return null;
    }, []);

    return { executeRecaptcha, isReady };
};
