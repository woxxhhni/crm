'use client';

import EmployeesDetailsComponent from "@/features/employees/components/employees-details.component";
import { usePermissionContext, PERMISSIONS } from '@/features/permissions';
import { useRouter } from 'next/navigation';
import { Result, Spin } from 'antd';
import { useTranslate } from '@/locales/use-locales';

export default function Page() {
  const { hasPermission, loading } = usePermissionContext();
  const router = useRouter();
  const { t } = useTranslate('common');

  if (loading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '60vh' }}>
        <Spin size="large" />
      </div>
    );
  }

  if (!hasPermission(PERMISSIONS.EMPLOYEE_VIEW_DETAIL)) {
    return (
      <Result
        status="403"
        title={t('accessDenied')}
        subTitle={t('noPermissionToViewEmployeeDetails')}
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

  return <EmployeesDetailsComponent />;
}
