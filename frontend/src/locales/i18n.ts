'use client';

import i18next, { type i18n as I18nType } from 'i18next';
import { initReactI18next } from 'react-i18next';
import resourcesToBackend from 'i18next-resources-to-backend';

import { cookieName, defaultNS, fallbackLng, i18nOptions, supportedLngs, type LanguageValue } from './locales-config';

function getStoredLanguage(): LanguageValue {
    if (typeof window === 'undefined') return fallbackLng;

    const localValue = window.localStorage.getItem(cookieName);
    if (supportedLngs.includes(localValue as LanguageValue)) {
        return localValue as LanguageValue;
    }

    const cookieValue = document.cookie
        .split('; ')
        .find((row) => row.startsWith(`${cookieName}=`))
        ?.split('=')[1];

    if (supportedLngs.includes(cookieValue as LanguageValue)) {
        return cookieValue as LanguageValue;
    }

    return fallbackLng;
}

// فقط یک‌بار init بشه
if (!i18next.isInitialized) {
    i18next
        .use(initReactI18next)
        .use(resourcesToBackend(
            (language: string, namespace: string) => import(`./langs/${language}/${namespace}.json`)
        ))
        .init(i18nOptions(getStoredLanguage(), defaultNS));
}

export default i18next as I18nType;
