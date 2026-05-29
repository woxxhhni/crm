'use client';

import "@ant-design/v5-patch-for-react-19";
import React, { useEffect, useMemo } from 'react';
import { ConfigProvider } from 'antd';
import enUS from 'antd/locale/en_US';
import zhCN from 'antd/locale/zh_CN';
import themeConfig from '@/config/theme';

import dayjs from 'dayjs';
import 'dayjs/locale/zh-cn';
import { useTranslate } from './use-locales';

type Props = { children: React.ReactNode };

export function LocalizationProvider({ children }: Props) {
    const { currentLang } = useTranslate();

    // در حال حاضر فقط انگلیسی فعال است
    const lang = currentLang.value;

    const { antLocale, direction } = useMemo(() => {
        return {
            antLocale: lang === 'zh' ? zhCN : enUS,
            direction: 'ltr' as const,
        };
    }, [lang]);

    dayjs.locale(currentLang.adapterLocale);

    useEffect(() => {
        document.documentElement.lang = lang === 'zh' ? 'zh-CN' : 'en';
        document.documentElement.dir = direction;
    }, [direction, lang]);

    return (
        <ConfigProvider locale={antLocale} direction={direction} theme={themeConfig}>
            {children}
        </ConfigProvider>
    );
}
