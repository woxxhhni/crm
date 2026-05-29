'use client';

import { useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { useRouter } from 'next/navigation';
import { allLangs } from './all-langs';
import { cookieName, fallbackLng, changeLangMessages as messages } from './locales-config';
import type { LanguageValue } from './locales-config';

export function useTranslate(ns?: string) {
    const router = useRouter();
    const { t, i18n } = useTranslation(ns);

    const fallback = allLangs.find((l) => l.value === fallbackLng)!;
    const resolvedLanguage = (i18n.resolvedLanguage || i18n.language) as LanguageValue;
    const currentLang = allLangs.find((l) => l.value === resolvedLanguage) || fallback;

    const onChangeLang = useCallback(
        async (newLang: LanguageValue) => {
            try {
                window.localStorage.setItem(cookieName, newLang);
                document.cookie = `${cookieName}=${newLang}; path=/; max-age=31536000; SameSite=Lax`;
                document.documentElement.lang = newLang === 'zh' ? 'zh-CN' : 'en';
                await i18n.changeLanguage(newLang);
                console.log(messages[newLang].success);
                router.refresh();
            } catch (error) {
                console.error(messages[newLang].error, error);
            }
        },
        [i18n, router]
    );

    return { t, i18n, currentLang, onChangeLang };
}
