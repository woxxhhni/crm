'use client';

import { Avatar, Button, Card, Typography, App, Space, Empty, Tooltip } from 'antd';
import { ArrowRightOutlined, DeleteOutlined, EditOutlined } from '@ant-design/icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { container } from 'tsyringe';
import { OrdersService } from '@/features/orders/services/orders.service';
import { useTranslate } from '@/locales/use-locales';
import LoadingSpinnerComponent from '@/components/loading/loading-spinner.component';
import { useState } from 'react';
import PaymentFormModalComponent from './payment-form-modal.component';
import { ConfirmDeleteModalComponent } from '@/components/modal/confirm-delete-modal.component';
import type { PaymentTransaction } from '@/api/orders/types';
import { useRouter } from 'next/navigation';
import { paths } from '@/routes/paths';
import { AppButton } from '@/components/button/app-button.component';
import DownloadExcelButtonComponent from './download-excel-button.component';
import { formatDateForDisplay } from '@/utils/date-utils';

function getPaymentTypeColor(paymentType: string) {
  const colors: Record<string, { bg: string; text: string; border: string }> = {
    Cash: { bg: '#E6F4FF', text: '#1677FF', border: '#91CAFF' },
    Transfer: { bg: '#E6F7FF', text: '#13C2C2', border: '#87E8DE' },
    Cheque: { bg: '#FFF7E6', text: '#FA8C16', border: '#FFD591' },
    Discount: { bg: '#FFF1F0', text: '#F5222D', border: '#FFA39E' },
  };
  return colors[paymentType] || { bg: '#F5F5F5', text: '#666666', border: '#D9D9D9' };
}

interface Props {
  orderId: string | number;
  type: 'client' | 'provider';
  orderStatus?: string;
}

export default function AccountingTabComponent({ orderId, type, orderStatus }: Props) {
  const { t } = useTranslate('orders');
  const { notification } = App.useApp();
  const queryClient = useQueryClient();
  const service = container.resolve(OrdersService);
  const router = useRouter();

  const [addModalOpen, setAddModalOpen] = useState(false);
  const [editModalOpen, setEditModalOpen] = useState(false);
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [selectedPaymentId, setSelectedPaymentId] = useState<number | null>(null);
  const [paymentToDelete, setPaymentToDelete] = useState<PaymentTransaction | null>(null);

  const queryKey = type === 'client' ? 'client-payments' : 'provider-payments';

  const { data: paymentsData, isLoading } = useQuery({
    queryKey: [queryKey, orderId],
    queryFn: () => (type === 'client' ? service.getClientPayments(orderId) : service.getProviderPayments(orderId)),
    enabled: !!orderId,
  });

  const deleteMutation = useMutation({
    mutationFn: async () => {
      if (!paymentToDelete) return;
      if (type === 'client') {
        await service.deleteClientPayment(orderId, paymentToDelete.id);
      } else {
        await service.deleteProviderPayment(orderId, paymentToDelete.id);
      }
    },
    onSuccess: async () => {
      notification.success({ message: t('paymentDeletedSuccessfully') });
      await queryClient.invalidateQueries({ queryKey: [queryKey, orderId] });
      setDeleteModalOpen(false);
      setPaymentToDelete(null);
    },
  });

  const handleViewPayment = (paymentId: number) => {
    const path = type === 'client' ? paths.panel.clientPaymentDetail(orderId, paymentId) : paths.panel.providerPaymentDetail(orderId, paymentId);
    router.push(path);
  };

  const handleDeleteClick = (payment: PaymentTransaction) => {
    setPaymentToDelete(payment);
    setDeleteModalOpen(true);
  };

  const handleEditPayment = (paymentId: number) => {
    setSelectedPaymentId(paymentId);
    setEditModalOpen(true);
  };

  if (isLoading) {
    return (
      <div className='flex justify-center items-center min-h-[300px]'>
        <LoadingSpinnerComponent />
      </div>
    );
  }

  return (
    <div>
      {/* Balance Summary - 4 Cards */}
      <div className='grid grid-cols-4 gap-4'>
        <Card className='rounded-2xl'>
          <Typography.Text className='text-gray-400 text-sm block'>{type === 'client' ? t('clientName') : t('providerName')}</Typography.Text>
          <Typography.Title level={4} className='!m-0 !mt-2'>
            {paymentsData?.name || '--'}
          </Typography.Title>
        </Card>
        <Card className='rounded-2xl'>
          <Typography.Text className='text-gray-400 text-sm block'>{t('totalBalance')}</Typography.Text>
          <Typography.Title level={4} className='!m-0 !mt-2'>
            {paymentsData?.totalBalance?.toLocaleString('en-US', { minimumFractionDigits: 2 }) || '0.00'} {paymentsData?.currency}
          </Typography.Title>
        </Card>
        <Card className='rounded-2xl'>
          <Typography.Text className='text-gray-400 text-sm block'>{t('paidAmount')}</Typography.Text>
          <Typography.Title level={4} className='!m-0 !mt-2 !text-green-600'>
            {paymentsData?.paidAmount?.toLocaleString('en-US', { minimumFractionDigits: 2 }) || '0.00'} {paymentsData?.currency}
          </Typography.Title>
        </Card>
        <Card className='rounded-2xl'>
          <Typography.Text className='text-gray-400 text-sm block'>{t('currentBalance')}</Typography.Text>
          <Typography.Title level={4} className='!m-0 !mt-2 !text-red-500'>
            {paymentsData?.currentBalance?.toLocaleString('en-US', { minimumFractionDigits: 2 }) || '0.00'} {paymentsData?.currency}
          </Typography.Title>
        </Card>
      </div>

      {/* Transactions Section Header */}
      <div className='flex justify-between items-center mt-6 mb-4'>
        <Typography.Title level={5} className='!m-0'>
          {t('transactions')}
        </Typography.Title>
        <Space>
          <DownloadExcelButtonComponent orderId={orderId} type={type} />
          {orderStatus?.toLowerCase() === 'inprogress' && (
            <AppButton onClick={() => setAddModalOpen(true)}>{t('addPayment')}</AppButton>
          )}
        </Space>
      </div>

      {/* Transactions List */}
      {paymentsData?.transactions?.length ? (
        <div className='space-y-3 p-4 rounded-xl border border-gray-200'>
          {paymentsData.transactions.map((transaction) => (
            <div key={transaction.id} className='flex justify-between items-center p-4 border border-gray-200 rounded-xl bg-white'>
              <div className='flex items-center gap-3'>
                <Avatar size={48} src={transaction.userProfileUrl || undefined} className='bg-gray-200' />
                <div>
                  <Typography.Text className='font-medium block'>{transaction.userFullName}</Typography.Text>
                  <span
                    className='inline-block px-3 py-0.5 text-xs rounded-full mt-1'
                    style={{
                      backgroundColor: getPaymentTypeColor(transaction.paymentType).bg,
                      color: getPaymentTypeColor(transaction.paymentType).text,
                      border: `1px solid ${getPaymentTypeColor(transaction.paymentType).border}`,
                    }}>
                    {transaction.paymentType}
                  </span>
                </div>
              </div>
              <div className='text-center'>
                <Typography.Text className='!text-gray-500'>{formatDateForDisplay(transaction.createdAt, 'YYYY-MM-DD')}</Typography.Text>
              </div>
              <div className='flex items-center gap-6'>
                <div className='text-right'>
                  <Typography.Text className='font-semibold block text-lg'>{transaction.amount?.toLocaleString('en-US', { minimumFractionDigits: 2 })}</Typography.Text>
                  <Typography.Text className='!text-gray-400 text-sm'>
                    {t('remaining')}: {transaction.remainingAmount?.toLocaleString('en-US', { minimumFractionDigits: 2 })}
                  </Typography.Text>
                </div>
                <Space size='small'>
                  <Tooltip title={t('viewDetails')}>
                    <Button
                      type='text'
                      shape='circle'
                      aria-label={t('viewDetails')}
                      onClick={() => handleViewPayment(transaction.id)}
                      icon={<ArrowRightOutlined />}
                      style={{ color: '#2563EB' }}
                    />
                  </Tooltip>
                  <Tooltip title={t('editPayment')}>
                    <Button
                      type='text'
                      shape='circle'
                      aria-label={t('editPayment')}
                      onClick={() => handleEditPayment(transaction.id)}
                      icon={<EditOutlined />}
                    />
                  </Tooltip>
                  <Tooltip title={t('deletePayment')}>
                    <Button
                      type='text'
                      shape='circle'
                      aria-label={t('deletePayment')}
                      icon={<DeleteOutlined className='!text-red-400' />}
                      onClick={() => handleDeleteClick(transaction)}
                    />
                  </Tooltip>
                </Space>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <div className='p-8 rounded-xl border border-gray-200 bg-white'>
          <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description={<Typography.Text className='text-gray-400'>{t('noTransactionsYet')}</Typography.Text>} />
        </div>
      )}

      <PaymentFormModalComponent open={addModalOpen} onClose={() => setAddModalOpen(false)} orderId={orderId} type={type} currency={paymentsData?.currency} />

      <PaymentFormModalComponent
        open={editModalOpen}
        onClose={() => {
          setEditModalOpen(false);
          setSelectedPaymentId(null);
        }}
        orderId={orderId}
        paymentId={selectedPaymentId}
        type={type}
        currency={paymentsData?.currency}
      />

      <ConfirmDeleteModalComponent
        open={deleteModalOpen}
        onCancel={() => {
          setDeleteModalOpen(false);
          setPaymentToDelete(null);
        }}
        onConfirm={() => deleteMutation.mutateAsync()}
        title={t('deletePayment')}
        loading={deleteMutation.isPending}
        description={
          <>
            {t('areYouSureYouWantToDeletePayment')} <strong>{paymentToDelete?.userFullName}</strong>?
          </>
        }
      />
    </div>
  );
}
