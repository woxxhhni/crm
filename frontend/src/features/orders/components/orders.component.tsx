'use client';

import 'reflect-metadata';
import { useState } from 'react';
import { Table, App, Tag, Card, Empty, Typography, Space, Button, Tooltip } from 'antd';
import { ArrowRightOutlined, PlusOutlined } from '@ant-design/icons';
import { Icon } from '@/components/iconify/iconify.component';
import { useTranslate } from '@/locales/use-locales';
import { ConfirmDeleteModalComponent } from '@/components/modal/confirm-delete-modal.component';
import { useModalStore } from '@/store/store';
import { useRouter } from 'next/navigation';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { container } from 'tsyringe';
import { OrdersService } from '@/features/orders/services/orders.service';
import { OrderRecord } from '@/api/orders/types';
import OrdersFormModalComponent from '@/features/orders/components/orders-form-modal.component';
import { ColumnsType } from 'antd/es/table';
import OrdersFiltersModalComponent from '@/features/orders/components/orders-filters-modal.component';
import { formatLogDate } from '@/features/orders/utils/log-type-config';
import { usePermissionContext, PERMISSIONS } from '@/features/permissions';
import { getTablePagination } from '@/config/table-pagination';

const { Title, Text } = Typography;

const OrdersComponent = () => {
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [editModalOpen, setEditModalOpen] = useState(false);
  const [selectedOrder, setSelectedOrder] = useState<OrderRecord | null>(null);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [filters, setFilters] = useState<Record<string, any>>({});

  const { notification } = App.useApp();
  const { t } = useTranslate('orders');
  const { t: tCommon } = useTranslate('common');
  const { activeModal, openModal, closeModal } = useModalStore();
  const router = useRouter();
  const queryClient = useQueryClient();
  const service = container.resolve(OrdersService);
  const { hasPermission } = usePermissionContext();

  // Permission checks
  const canCreateOrder = hasPermission(PERMISSIONS.ORDER_CREATE);
  const canEditOrder = hasPermission(PERMISSIONS.ORDER_EDIT);
  const canDeleteOrder = hasPermission(PERMISSIONS.ORDER_DELETE);

  const getStatusLabel = (status: string) => {
    const statusLabelMap: Record<string, string> = {
      completed: t('completed'),
      inprogress: t('inProgress'),
      suspended: t('suspended'),
      canceled: t('canceled'),
    };

    return statusLabelMap[status?.toLowerCase()] || status;
  };

  const { data: ordersData, isLoading } = useQuery({
    queryKey: ['orders-list', page, pageSize, filters],
    queryFn: async () => await service.list(page, pageSize, filters),
  });

  const deleteMutation = useMutation({
    mutationFn: async ({ id, label }: { id: number | string; label: string }) => {
      await service.delete(id, label);
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['orders-list'] });
      notification.success({ message: t('orderDeletedSuccessfully') });
    },
  });

  const handleConfirmDelete = async () => {
    if (!selectedOrder) return;
    setDeleteModalOpen(false);
    deleteMutation.mutate({ id: selectedOrder.id as number, label: 'order' });
  };

  const handleDeleteClick = (record: OrderRecord) => {
    setSelectedOrder(record);
    setDeleteModalOpen(true);
  };

  const columns: ColumnsType<OrderRecord> = [
    { title: t('orderNumber'), dataIndex: 'orderNumber', key: 'orderNumber', width: 140 },
    { title: t('orderDate'), dataIndex: 'orderDate', key: 'orderDate', width: 120, render: (date) => date ? formatLogDate(date) : '' },
    { title: t('title'), dataIndex: 'title', key: 'title', ellipsis: true },
    { title: t('client'), dataIndex: 'clientName', key: 'clientName', ellipsis: true },
    { title: t('provider'), dataIndex: 'providerName', key: 'providerName', ellipsis: true },
    {
      title: t('status'),
      dataIndex: 'status',
      key: 'status',
      width: 120,
      align: 'center',
      render: (status: string) => {
        const statusColorMap: Record<string, string> = {
          completed: 'success',
          inprogress: 'blue',
          suspended: 'warning',
          canceled: 'error',
        };

        return <Tag color={statusColorMap[status?.toLowerCase()] || 'default'}>{getStatusLabel(status)}</Tag>;
      },
    },
    {
      title: t('actions'),
      key: 'actions',
      width: 120,
      render: (_: any, record: OrderRecord) => (
        <div className='flex items-center text-gray-500'>
          <Tooltip title={tCommon('openDetails')}>
            <Button
              type='text'
              shape='circle'
              aria-label={tCommon('openDetails')}
              icon={<ArrowRightOutlined />}
              onClick={() => router.push(`/panel/orders/details/${record.id}`)}
              style={{ color: '#2563EB' }}
            />
          </Tooltip>
          {canEditOrder && (
            <Tooltip title={tCommon('edit')}>
              <div
                role='button'
                aria-label={tCommon('edit')}
                className='w-10 h-10 flex items-center justify-center cursor-pointer hover:bg-gray-100 rounded-lg transition-colors'
                onClick={() => {
                  setSelectedOrder(record);
                  setEditModalOpen(true);
                }}>
                <Icon icon='edit' style={{ width: 18 }} />
              </div>
            </Tooltip>
          )}
        </div>
      ),
    },
  ];

  return (
    <>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
        <div>
          <Title level={4} style={{ margin: 0 }}>{t('orders')}</Title>
          <Text type="secondary" style={{ fontSize: 13 }}>
            {t('ordersPageSubtitle')}
          </Text>
        </div>
        <Space>
          <Button onClick={() => openModal('orderFilter')}>
            {t('filter')}
          </Button>
          {canCreateOrder && (
            <Button type="primary" icon={<PlusOutlined />} onClick={() => openModal('addOrder')}>
              {t('addOrder')}
            </Button>
          )}
        </Space>
      </div>

      <Card styles={{ body: { padding: 0 } }}>
        <Table
          dataSource={ordersData?.items ?? []}
          columns={columns}
          rowKey={(record) => String(record.id)}
          loading={isLoading}
          size="middle"
          pagination={getTablePagination({
            current: ordersData?.page ?? page,
            total: ordersData?.total ?? 0,
            pageSize: ordersData?.pageSize ?? pageSize,
            onChange: (newPage, newSize) => {
              setPage(newPage);
              setPageSize(newSize);
            },
          })}
          scroll={{ x: 'max-content' }}
          locale={{
            emptyText: (
              <Empty
                image={Empty.PRESENTED_IMAGE_SIMPLE}
                description={t('noOrdersYet')}
              />
            ),
          }}
        />
      </Card>

      {canDeleteOrder && (
        <ConfirmDeleteModalComponent
          open={deleteModalOpen}
          onCancel={() => setDeleteModalOpen(false)}
          onConfirm={handleConfirmDelete}
          title={t('deleteOrder')}
          description={<>{t('areYouSureYouWantToDeleteThisOrder')}</>}
        />
      )}

      {activeModal === 'addOrder' && canCreateOrder && <OrdersFormModalComponent open={activeModal === 'addOrder'} onClose={closeModal} isCreateForm />}

      {editModalOpen && canEditOrder && <OrdersFormModalComponent open={editModalOpen} onClose={() => setEditModalOpen(false)} initialValues={selectedOrder} />}

      <OrdersFiltersModalComponent
        open={activeModal === 'orderFilter'}
        onClose={closeModal}
        currentFilters={filters}
        onApply={(appliedFilters) => {
          setFilters(appliedFilters);
          queryClient.invalidateQueries({ queryKey: ['orders-list'] });
        }}
      />
    </>
  );
};

export default OrdersComponent;
