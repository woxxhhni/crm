'use client';

import { Table, Button, Space, Typography, Tooltip, Empty, App, Card } from 'antd';
import { ArrowRightOutlined, PlusOutlined, DeleteOutlined } from '@ant-design/icons';
import { useState } from 'react';
import { useTranslate } from '@/locales/use-locales';
import { ExtraProviderRecord } from '@/api/orders/types';
import AddExtraProviderModal from './add-extra-provider-modal.component';
import { ConfirmDeleteModalComponent } from '@/components/modal/confirm-delete-modal.component';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { container } from 'tsyringe';
import { OrdersService } from '@/features/orders/services/orders.service';
import { useRouter } from 'next/navigation';
import { paths } from '@/routes/paths';

interface Props {
    orderId: string | number;
    extraProviders?: ExtraProviderRecord[];
    loading?: boolean;
}

export default function ExtraProvidersTabComponent({ orderId, extraProviders, loading }: Props) {
    const { t } = useTranslate('orders');
    const [isAddModalOpen, setIsAddModalOpen] = useState(false);
    const [deleteModalOpen, setDeleteModalOpen] = useState(false);
    const [providerToDelete, setProviderToDelete] = useState<ExtraProviderRecord | null>(null);

    const service = container.resolve(OrdersService);
    const queryClient = useQueryClient();
    const { notification } = App.useApp();
    const router = useRouter();

    const deleteMutation = useMutation({
        mutationFn: async () => {
            if (providerToDelete) {
                await service.removeExtraProvider(orderId, providerToDelete.id); // Assuming providerId in remove is the Link ID, wait. API says "Remove Extra Provider" ... "/orders/{id}/extra-providers/{epId}". extraProviders list has "id".
            }
        },
        onSuccess: () => {
            notification.success({ message: t('extraProviderRemovedSuccessfully') });
            queryClient.invalidateQueries({ queryKey: ['order-edit-details', orderId] });
            setDeleteModalOpen(false);
            setProviderToDelete(null);
        },
        onError: () => {
            notification.error({ message: t('failedToRemoveExtraProvider') });
        }
    });

    const columns = [
        {
            title: t('provider'),
            dataIndex: 'providerName',
            key: 'providerName',
        },
        {
            title: t('amount'),
            key: 'amount',
            render: (text: string, record: ExtraProviderRecord) => (
                <span>
                    {new Intl.NumberFormat('en-US', {
                        style: 'decimal',
                        minimumFractionDigits: 2,
                        maximumFractionDigits: 2,
                    }).format(record.amount)} {record.currency}
                </span>
            ),
        },
        {
            title: t('paidAmount'),
            key: 'paidAmount',
            render: (text: string, record: ExtraProviderRecord) => (
                <span className="text-green-600">
                    {new Intl.NumberFormat('en-US', {
                        style: 'decimal',
                        minimumFractionDigits: 2,
                        maximumFractionDigits: 2,
                    }).format(record.paidAmount || 0)} {record.currency}
                </span>
            ),
        },
        {
            title: t('remaining'),
            key: 'remaining',
            render: (text: string, record: ExtraProviderRecord) => {
                const paid = record.paidAmount || 0;
                const remaining = record.amount - paid;
                return (
                    <span className="text-red-500">
                        {new Intl.NumberFormat('en-US', {
                            style: 'decimal',
                            minimumFractionDigits: 2,
                            maximumFractionDigits: 2,
                        }).format(remaining)} {record.currency}
                    </span>
                )
            },
        },
        {
            title: t('actions'),
            key: 'actions',
            width: 120,
            render: (_: any, record: ExtraProviderRecord) => (
                <Space>
                    <Tooltip title={t('viewDetails')}>
                        <Button
                            type="text"
                            shape="circle"
                            aria-label={t('viewDetails')}
                            icon={<ArrowRightOutlined />}
                            style={{ color: '#2563EB' }}
                            onClick={() => {
                                // Navigate to detail page
                                router.push(paths.panel.extraProviderDetail(orderId, record.id));
                            }}
                        />
                    </Tooltip>
                    <Tooltip title={t('remove')}>
                        <Button
                            type="text"
                            danger
                            shape="circle"
                            aria-label={t('remove')}
                            icon={<DeleteOutlined />}
                            onClick={() => {
                                setProviderToDelete(record);
                                setDeleteModalOpen(true);
                            }}
                        />
                    </Tooltip>
                </Space>
            ),
        },
    ];

    return (
        <div className="space-y-4">
            <div className="flex justify-between items-center mb-4">
                <Typography.Title level={4} className="!m-0">{t('extraProviders')}</Typography.Title>
                <Button
                    type="primary"
                    icon={<PlusOutlined />}
                    onClick={() => setIsAddModalOpen(true)}
                >
                    {t('addExtraProvider')}
                </Button>
            </div>

            <Table
                dataSource={extraProviders}
                columns={columns}
                rowKey="id"
                loading={loading}
                pagination={false}
                locale={{ emptyText: <Empty description={t('noExtraProviders')} /> }}
            />

            <AddExtraProviderModal
                open={isAddModalOpen}
                onCancel={() => setIsAddModalOpen(false)}
                orderId={orderId}
            />

            <ConfirmDeleteModalComponent
                open={deleteModalOpen}
                title={t('removeExtraProvider')}
                description={t('areYouSureRemoveExtraProvider')}
                onConfirm={() => deleteMutation.mutate()}
                onCancel={() => setDeleteModalOpen(false)}
                loading={deleteMutation.isPending}
            />
        </div>
    );
}
