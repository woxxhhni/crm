import type { LanguageValue } from './locales-config';

export const allLangs: {
    label: string;
    adapterLocale: string;
    value: LanguageValue;
}[] = [
    { label: 'English', adapterLocale: 'en', value: 'en' },
    { label: '中文', adapterLocale: 'zh-cn', value: 'zh' },
];
