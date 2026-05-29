// src/locales/server.ts
import { cache } from 'react';
import { createInstance } from 'i18next';
import { initReactI18next } from 'react-i18next/initReactI18next';
import resourcesToBackend from 'i18next-resources-to-backend';
import { cookies } from 'next/headers';

import { defaultNS, cookieName, i18nOptions, fallbackLng } from './locales-config';
import type { LanguageValue } from './locales-config';

// ----------------------------------------------------------------------
export async function detectLanguage() {
    const cookieStore = (await Promise.resolve(cookies())) as any;
    const language = cookieStore.get(cookieName)?.value ?? fallbackLng;
    return language as LanguageValue;
}

// ----------------------------------------------------------------------
export const getServerTranslations = cache(async (ns = defaultNS, options = {}) => {
    const language = await detectLanguage();
    const i18nextInstance = await initServerI18next(language, ns);
    return {
        t: i18nextInstance.getFixedT(
            language,
            Array.isArray(ns) ? ns[0] : ns,
            (options as Record<string, any>).keyPrefix
        ),
        i18n: i18nextInstance,
    };
});

// ----------------------------------------------------------------------

const initServerI18next = async (language: string, namespace: string) => {
    const i18nInstance = createInstance();

    await i18nInstance
        .use(initReactI18next)
        .use(
            resourcesToBackend((lang: string, ns: string) =>
                import(`./langs/${lang}/${ns}.json`)
            )
        )
        .init(i18nOptions(language, namespace));

    return i18nInstance;
};
