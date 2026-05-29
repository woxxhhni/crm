import type { InitOptions } from 'i18next';

export type LanguageValue = 'en' | 'zh';

export const fallbackLng: LanguageValue = 'en';
export const cookieName = 'NEXT_LOCALE';
export const defaultNS = 'common';
export const supportedLngs: LanguageValue[] = ['en', 'zh'];

export const i18nOptions = (lng: string, ns: string): InitOptions => ({
    lng,
    fallbackLng,
    ns,
    defaultNS,
    interpolation: { escapeValue: false },
    supportedLngs,
});

export const changeLangMessages = {
    en: {
        loading: 'Changing language...',
        success: 'Language changed to English',
        error: 'Error changing language',
    },
    zh: {
        loading: '正在切换语言...',
        success: '语言已切换为中文',
        error: '语言切换失败',
    },
};
