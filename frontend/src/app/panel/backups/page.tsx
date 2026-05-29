'use client';

import { BackupsComponent } from '@/features/backups/components/backups.component';
import { RoleGuard } from '@/features/permissions';
import { useRouter } from 'next/navigation';
import { useEffect } from 'react';
import { usePermissionContext } from '@/features/permissions';
import { Result, Spin } from 'antd';
import { useTranslate } from '@/locales/use-locales';

export default function Page() {
  const { role, loading } = usePermissionContext();
  const router = useRouter();
  const { t } = useTranslate('common');

  if (loading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '60vh' }}>
        <Spin size="large" />
      </div>
    );
  }

  if (role !== 'admin') {
    return (
      <Result
        status="403"
        title={t('accessDenied')}
        subTitle={t('noPermissionToBackups')}
        extra={
          <button
            onClick={() => router.push('/panel/orders')}
            style={{
              padding: '8px 24px',
              background: '#1677ff',
              color: '#fff',
              border: 'none',
              borderRadius: '6px',
              cursor: 'pointer',
              fontSize: '14px',
            }}
          >
            {t('goToOrders')}
          </button>
        }
      />
    );
  }

  return <BackupsComponent />;
}
