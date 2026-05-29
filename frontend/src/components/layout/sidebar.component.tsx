'use client';

import { useMemo } from 'react';
import { Typography, Tooltip } from 'antd';
import { LogoutOutlined } from '@ant-design/icons';
import Link from 'next/link';
import { usePathname, useRouter } from 'next/navigation';
import Image from 'next/image';
import { Icon } from '@/components/iconify/iconify.component';
import { setSession } from '@/features/auth/sign-in/context/jwt/utils';
import { useAuthContext } from '@/features/auth/sign-in/hooks';
import { useTranslate } from '@/locales/use-locales';
import { paths } from '@/routes/paths';
import { usePermissionContext, PERMISSIONS } from '@/features/permissions';

export const SidebarComponent = () => {
  const pathname = usePathname();
  const router = useRouter();
  const { checkUserSession } = useAuthContext();
  const { t } = useTranslate('common');
  const { hasPermission, role } = usePermissionContext();

  const canViewEmployees = hasPermission(PERMISSIONS.EMPLOYEE_VIEW);
  const canViewDashboard = role === 'admin' || role === 'manager';

  const isAdmin = role === 'admin';

  const menuItems = useMemo(
    () => [
      ...(canViewDashboard ? [{ key: '/panel/dashboard', label: t('dashboard'), icon: <Icon icon='dashboard' /> }] : []),
      { key: '/panel/orders', label: t('orders'), icon: <Icon icon='list' /> },
      { key: '/panel/clients', label: t('clients'), icon: <Icon icon='team' /> },
      { key: '/panel/providers', label: t('providers'), icon: <Icon icon='provider' /> },
      ...(canViewEmployees ? [{ key: '/panel/employees', label: t('employees'), icon: <Icon icon='formPage' /> }] : []),
      ...(isAdmin ? [{ key: '/panel/backups', label: t('backups'), icon: <Icon icon='backup' /> }] : []),
    ],
    [canViewEmployees, canViewDashboard, isAdmin, t]
  );

  const selectedKey = useMemo(() => {
    const matchedItem = menuItems.find((item) => pathname.startsWith(item.key));
    return matchedItem?.key || pathname;
  }, [pathname, menuItems]);

  const handleLogout = async () => {
    await setSession(null);
    await checkUserSession?.();
    router.replace(paths.auth.signIn);
  };

  return (
    <div
      className='flex flex-col h-full'
      style={{ background: '#0F1729' }}
    >
      {/* ── Brand ── */}
      <div className='flex items-center gap-3 px-5 py-5'>
        <div className='w-9 h-9 rounded-lg overflow-hidden flex-shrink-0'>
          <Image
            src='/assets/logo/logo.png'
            alt='CLS logo'
            width={36}
            height={36}
            style={{ width: 36, height: 36, objectFit: 'cover' }}
          />
        </div>
        <div className='flex flex-col'>
          <span className='text-white text-[15px] font-semibold leading-tight'>
            CLS
          </span>
          <span className='text-[11px] leading-tight' style={{ color: '#94A3B8' }}>
            {t('appSubtitle')}
          </span>
        </div>
      </div>

      {/* Divider */}
      <div className='mx-4 border-t' style={{ borderColor: 'rgba(255,255,255,0.08)' }} />

      {/* ── Navigation ── */}
      <nav className='flex-1 py-3 px-2'>
        {menuItems.map((item) => {
          const isActive = pathname.startsWith(item.key);
          return (
            <Link
              key={item.key}
              href={item.key}
              className='no-underline block mb-0.5'
            >
              <div
                className='flex items-center gap-3 px-3 py-2.5 rounded-lg transition-all duration-200'
                style={{
                  background: isActive ? 'rgba(59, 130, 246, 0.18)' : 'transparent',
                  fontWeight: isActive ? 600 : 400,
                }}
                onMouseEnter={(e) => {
                  if (!isActive) {
                    e.currentTarget.style.background = 'rgba(255,255,255,0.06)';
                  }
                }}
                onMouseLeave={(e) => {
                  if (!isActive) {
                    e.currentTarget.style.background = 'transparent';
                  }
                }}
              >
                <span
                  className='text-[18px] flex items-center'
                  style={{ filter: 'brightness(0) invert(1)', opacity: isActive ? 1 : 0.85 }}
                >{item.icon}</span>
                <span
                  className='text-[13px]'
                  style={{ color: isActive ? '#FFFFFF' : '#E2E8F0' }}
                >{item.label}</span>
              </div>
            </Link>
          );
        })}
      </nav>

      {/* ── Logout ── */}
      <div className='px-3 pb-4 pt-2 border-t' style={{ borderColor: 'rgba(255,255,255,0.08)' }}>
        <Tooltip title={t('logout')} placement='right'>
          <button
            onClick={handleLogout}
            className='flex items-center gap-3 px-3 py-2.5 rounded-lg w-full cursor-pointer border-0 bg-transparent text-left transition-all duration-200'
            style={{ color: '#E2E8F0' }}
            onMouseEnter={(e) => {
              e.currentTarget.style.color = '#F87171';
              e.currentTarget.style.background = 'rgba(239,68,68,0.1)';
            }}
            onMouseLeave={(e) => {
              e.currentTarget.style.color = '#E2E8F0';
              e.currentTarget.style.background = 'transparent';
            }}
          >
            <LogoutOutlined className='text-[16px]' />
            <span className='text-[13px]'>{t('logout')}</span>
          </button>
        </Tooltip>
      </div>
    </div>
  );
};
