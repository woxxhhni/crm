'use client';
import 'reflect-metadata';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { FormProvider, useForm } from 'react-hook-form';
import { Button, Select, Typography, App } from 'antd';
import { GlobalOutlined } from '@ant-design/icons';
import Image from 'next/image';
import { useMutation } from '@tanstack/react-query';
import { container } from 'tsyringe';

import { useTranslate } from '@/locales/use-locales';
import { AppTextField } from '@/components/form/app-text-field/app-text-field.component';
import { AuthService } from '@/features/auth/sign-in/services/auth.service';
import { setSession } from '@/features/auth/sign-in/context/jwt';
import { useAuthContext, useRecaptcha } from '@/features/auth/sign-in/hooks';
import { allLangs } from '@/locales/all-langs';
import type { LanguageValue } from '@/locales/locales-config';

const { Title, Text } = Typography;

export default function SignInComponent() {
  const { t, currentLang, onChangeLang } = useTranslate('auth');
  const { notification } = App.useApp();
  const { checkUserSession } = useAuthContext();

  const schema = z.object({
    email: z.string().min(1, { error: t('enterValidEmail') }),
    password: z.string().min(1, t('enterValidPassword')),
  });
  type FormData = z.infer<typeof schema>;

  const methods = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      email: '',
      password: '',
    },
  });

  const { executeRecaptcha } = useRecaptcha();
  const authService = container.resolve(AuthService);

  const loginMutation = useMutation({
    mutationFn: async (payload: FormData) => {
      const token = await executeRecaptcha('login');
      return authService.login(payload.email, payload.password, token || undefined);
    },
    onSuccess: async (res) => {
      setSession(res.token);
      await checkUserSession?.();
    },
  });
  const onSubmit = (values: FormData) => loginMutation.mutate(values);

  return (
    <div
      className='min-h-screen flex items-center justify-center px-4'
      style={{
        background: 'linear-gradient(135deg, #0F172A 0%, #1E3A5F 40%, #1B3A5C 70%, #0B1120 100%)',
      }}
    >
      {/* Subtle background pattern */}
      <div
        className='absolute inset-0 opacity-[0.03]'
        style={{
          backgroundImage: `radial-gradient(circle at 1px 1px, rgba(255,255,255,0.3) 1px, transparent 0)`,
          backgroundSize: '40px 40px',
        }}
      />

      <div className='absolute right-5 top-5 flex items-center gap-1 rounded-lg border border-white/15 bg-white/10 px-2 h-9 backdrop-blur'>
        <GlobalOutlined style={{ color: '#fff' }} />
        <Select
          aria-label='Language'
          variant='borderless'
          value={currentLang.value}
          options={allLangs.map((lang) => ({ value: lang.value, label: lang.label }))}
          onChange={(value) => onChangeLang(value as LanguageValue)}
          style={{ width: 106 }}
        />
      </div>

      <div
        className='relative w-full max-w-[420px] rounded-2xl p-8 sm:p-10'
        style={{
          background: 'var(--surface)',
          boxShadow: '0 25px 60px rgba(0,0,0,0.3)',
        }}
      >
        {/* Logo & Brand */}
        <div className='flex flex-col items-center gap-3 mb-8'>
          <div className='w-16 h-16 rounded-xl overflow-hidden shadow-sm'>
            <Image src='/assets/logo/logo.png' alt='CLS Logo' width={64} height={64} style={{ borderRadius: 12 }} />
          </div>
          <div className='text-center'>
            <Title level={3} className='!mb-0' style={{ color: 'var(--text)' }}>
              CLS
            </Title>
            <Text className='text-xs' style={{ color: 'var(--text-muted)' }}>
              Canadian Logistics Solution
            </Text>
          </div>
        </div>

        {/* Divider */}
        <div className='border-t mb-6' style={{ borderColor: 'var(--border)' }} />

        {/* Form */}
        <FormProvider {...methods}>
          <form onSubmit={methods.handleSubmit(onSubmit)} className='space-y-5'>
            <AppTextField name='email' label={t('email')} placeholder={t('email')} />
            <AppTextField name='password' type='password' label={t('password')} placeholder={t('enterPassword')} />
            <Button
              type='primary'
              htmlType='submit'
              loading={loginMutation.isPending}
              className='w-full h-11 mt-2 rounded-lg font-medium'
              size='large'
            >
              {t('login')}
            </Button>
          </form>
        </FormProvider>
      </div>
    </div>
  );
}
