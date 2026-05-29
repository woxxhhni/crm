'use client';
import 'reflect-metadata';
import { Button, Select, Tooltip } from 'antd';
import { GlobalOutlined, MenuOutlined } from '@ant-design/icons';
import { usePathname, useParams } from 'next/navigation';
import { useWindowSize } from '@/hooks/useWindowSize';
import { AppButton } from '@/components/button/app-button.component';
import { AppBreadcrumb } from '@/components/breadcrumb/app-breadcrumb.component';
import { useModalStore } from '@/store/store';
import { useTranslate } from '@/locales/use-locales';
import { useQuery } from '@tanstack/react-query';
import { container } from 'tsyringe';
import { OrdersService } from '@/features/orders/services/orders.service';
import { ClientsService } from '@/features/clients/services/clients.service';
import { ProvidersService } from '@/features/providers/services/providers.service';
import { EmployeesService } from '@/features/employees/services/employees.service';
import { useState, useMemo } from 'react';
import OrderActionModalComponent from '@/features/orders/components/order-action-modal.component';
import { usePermissionContext, PERMISSIONS } from '@/features/permissions';
import { allLangs } from '@/locales/all-langs';
import type { LanguageValue } from '@/locales/locales-config';

import type { ActivityLog } from '@/api/orders/types';

export const HeaderComponent = ({ onMenu }: { onMenu: () => void }) => {
  const pathname = usePathname();
  const params = useParams();
  const { isMobile } = useWindowSize();
  const { openModal } = useModalStore();
  const { t, currentLang, onChangeLang } = useTranslate('common');

  const ordersService = useMemo(() => container.resolve(OrdersService), []);
  const clientsService = useMemo(() => container.resolve(ClientsService), []);
  const providersService = useMemo(() => container.resolve(ProvidersService), []);
  const employeesService = useMemo(() => container.resolve(EmployeesService), []);
  const [orderActionModalOpen, setOrderActionModalOpen] = useState(false);
  const [orderActionType, setOrderActionType] = useState<'complete' | 'cancel' | 'suspend' | 'unsuspend' | null>(null);
  const { hasPermission } = usePermissionContext();

  // Permission checks
  const canCreateOrder = hasPermission(PERMISSIONS.ORDER_CREATE);
  const canCreateEmployee = hasPermission(PERMISSIONS.EMPLOYEE_CREATE);
  const canChangeOrderStatus = hasPermission(PERMISSIONS.ORDER_CHANGE_STATUS);
  const canCreateClient = hasPermission(PERMISSIONS.CLIENT_CREATE);
  const canCreateProvider = hasPermission(PERMISSIONS.PROVIDER_CREATE);

  // Detect route patterns
  const orderId = params.orderId as string;
  const logId = params.logId as string;
  const paymentId = params.paymentId as string;
  const epId = params.epId as string;
  const employeeId = params.id as string;

  const isOrderDetailsPage = pathname?.match(/^\/panel\/orders\/details\/\d+$/);
  const isOrderLogPage = pathname?.includes('/logs/') && orderId && logId;
  const isClientPaymentPage = pathname?.includes('/client-payments/') && orderId && paymentId;
  const isProviderPaymentPage = pathname?.includes('/provider-payments/') && orderId && paymentId;
  const isExtraProviderPage = pathname?.includes('/extra-providers/') && orderId && epId;
  const isClientDetailsPage = pathname?.match(/^\/panel\/clients\/details\/\d+$/);
  const isProviderDetailsPage = pathname?.match(/^\/panel\/providers\/details\/\d+$/);
  const isEmployeeDetailsPage = pathname?.match(/^\/panel\/employees\/details\/\d+$/);

  // Get order details (used by order details page and nested pages)
  const { data: orderDetails } = useQuery({
    queryKey: ['order-edit-details', orderId || employeeId],
    queryFn: () => ordersService.getDetails(orderId || employeeId),
    enabled: !!(isOrderDetailsPage || isOrderLogPage || isClientPaymentPage || isProviderPaymentPage || isExtraProviderPage) && !!(orderId || employeeId),
  });

  // Get client details
  const { data: clientDetails } = useQuery({
    queryKey: ['client-details', Number(employeeId)],
    queryFn: () => clientsService.getDetails(Number(employeeId)),
    enabled: !!isClientDetailsPage && !!employeeId,
  });

  // Get provider details
  const { data: providerDetails } = useQuery({
    queryKey: ['provider-details', Number(employeeId)],
    queryFn: () => providersService.getDetails(Number(employeeId)),
    enabled: !!isProviderDetailsPage && !!employeeId,
  });

  // Get employee details
  const { data: employeeDetails } = useQuery({
    queryKey: ['employee-details', employeeId],
    queryFn: () => employeesService.getDetails(employeeId),
    enabled: !!isEmployeeDetailsPage && !!employeeId,
  });

  // Get log details from order details
  const logDetails = useMemo(() => {
    if (!isOrderLogPage || !orderDetails?.steps || !logId) return null;
    for (const step of orderDetails.steps) {
      const log = step.logs.find((l: ActivityLog) => l.id === Number(logId));
      if (log) return log;
    }
    return null;
  }, [isOrderLogPage, orderDetails?.steps, logId]);

  // Get payment details for nested payment pages
  const { data: paymentDetails } = useQuery({
    queryKey: [
      isClientPaymentPage ? 'client-payment-detail' : 'provider-payment-detail',
      orderId,
      paymentId,
    ],
    queryFn: () =>
      isClientPaymentPage
        ? ordersService.getClientPaymentDetail(orderId, paymentId)
        : ordersService.getProviderPaymentDetail(orderId, paymentId),
    enabled: !!(isClientPaymentPage || isProviderPaymentPage) && !!orderId && !!paymentId,
  });

  // Get extra provider details for nested extra-provider pages
  const { data: extraProviderSummary } = useQuery({
    queryKey: ['extra-provider-payments', orderId, epId],
    queryFn: () => ordersService.getExtraProviderPaymentSummary(orderId, epId),
    enabled: !!isExtraProviderPage && !!orderId && !!epId,
  });

  const extraProviderDetails = useMemo(() => {
    if (extraProviderSummary?.name) return { name: extraProviderSummary.name };
    return undefined;
  }, [extraProviderSummary?.name]);

  // Order action helpers
  const handleOrderAction = (actionType: 'complete' | 'cancel' | 'suspend' | 'unsuspend') => {
    setOrderActionType(actionType);
    setOrderActionModalOpen(true);
  };

  const getOrderActionButtons = () => {
    // Status actions are now rendered inside the order detail card itself
    return null;
  };

  return (
    <>
      <header
        className='px-4 sm:px-6 py-3 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 border-b'
        style={{
          background: 'var(--header-bg)',
          borderColor: 'var(--border)',
          backdropFilter: 'blur(12px)',
          WebkitBackdropFilter: 'blur(12px)',
        }}
      >
        <div className='flex flex-col gap-1.5'>
          {isMobile && <Button type='text' icon={<MenuOutlined />} onClick={onMenu} className='lg:hidden mb-1' />}
          <AppBreadcrumb
            pathname={pathname || ''}
            orderDetails={orderDetails}
            clientDetails={clientDetails}
            providerDetails={providerDetails}
            employeeDetails={employeeDetails}
            logDetails={logDetails}
            paymentDetails={paymentDetails}
            extraProviderDetails={extraProviderDetails}
            isClientPaymentPage={!!isClientPaymentPage}
          />
        </div>

        <div className='flex items-center gap-2 justify-end sm:justify-center sm:mt-0 mt-2'>
          {getOrderActionButtons()}
          <Tooltip title={t('language')}>
            <div
              className='flex items-center gap-1 rounded-lg border px-2 h-9'
              style={{ borderColor: 'var(--border)', background: 'var(--surface)' }}
            >
              <GlobalOutlined style={{ color: 'var(--text-muted)' }} />
              <Select
                aria-label={t('language')}
                variant='borderless'
                value={currentLang.value}
                options={allLangs.map((lang) => ({ value: lang.value, label: lang.label }))}
                onChange={(value) => onChangeLang(value as LanguageValue)}
                style={{ width: isMobile ? 82 : 106 }}
              />
            </div>
          </Tooltip>
        </div>
      </header>

      {/* Order Action Modal */}
      <OrderActionModalComponent
        open={orderActionModalOpen}
        onClose={() => {
          setOrderActionModalOpen(false);
          setOrderActionType(null);
        }}
        orderId={orderId || employeeId}
        actionType={orderActionType}
      />
    </>
  );
};
