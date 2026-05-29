'use client';

import 'reflect-metadata';
import { useState } from 'react';
import { Table, App, Card, Empty, Typography, Space, Button, Tooltip } from 'antd';
import { ArrowRightOutlined, PlusOutlined } from '@ant-design/icons';
import { Icon } from '@/components/iconify/iconify.component';
import { useTranslate } from '@/locales/use-locales';
import { ConfirmDeleteModalComponent } from '@/components/modal/confirm-delete-modal.component';
import { useModalStore } from '@/store/store';
import ProvidersFormModalComponent from '@/features/providers/components/providers-form-modal.component';
import { useRouter } from 'next/navigation';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { container } from 'tsyringe';
import { ProvidersService } from '@/features/providers/services/providers.service';
import ProvidersFiltersModalComponent from "@/features/providers/components/providers-filters-modal.component";
import { usePermissionContext, PERMISSIONS } from '@/features/permissions';
import { getTablePagination } from '@/config/table-pagination';

const { Title, Text } = Typography;

export default function ProvidersComponent() {
    const [deleteModalOpen, setDeleteModalOpen] = useState(false);
    const [editModalOpen, setEditModalOpen] = useState(false);
    const [selectedProvider, setSelectedProvider] = useState<any | null>(null);
    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(10);
    const [filters, setFilters] = useState<Record<string, any>>({});

    const { notification } = App.useApp();
    const { t } = useTranslate('providers');
    const { t: tCommon } = useTranslate('common');
    const { activeModal, openModal, closeModal } = useModalStore();
    const router = useRouter();
    const queryClient = useQueryClient();
    const service = container.resolve(ProvidersService);

    const { hasPermission } = usePermissionContext();
    const canCreateProvider = hasPermission(PERMISSIONS.PROVIDER_CREATE);
    const canEditProvider = hasPermission(PERMISSIONS.PROVIDER_EDIT);
    const canDeleteProvider = hasPermission(PERMISSIONS.PROVIDER_DELETE);

    const { data: providersData, isLoading } = useQuery({
        queryKey: ['providers-list', page, pageSize, filters],
        queryFn: async () => await service.list(page, pageSize, filters),
    });

    const deleteMutation = useMutation({
        mutationFn: async (id: number) => {
            await service.delete(id);
        },
        onSuccess: async () => {
            await queryClient.invalidateQueries({ queryKey: ['providers-list'] });
            notification.success({ message: t('providerDeletedSuccessfully') });
        },
    });

    const handleDeleteClick = (record: any) => {
        setSelectedProvider(record);
        setDeleteModalOpen(true);
    };

    const handleConfirmDelete = async () => {
        if (!selectedProvider) return;
        setDeleteModalOpen(false);
        deleteMutation.mutate(Number(selectedProvider.id));
    };

    const columns = [
        { title: t('id'), dataIndex: 'id', key: 'id', width: 70 },
        { title: t('name'), dataIndex: 'name', key: 'name', ellipsis: true },
        { title: t('email'), dataIndex: 'email', key: 'email', ellipsis: true },
        { title: t('website'), dataIndex: 'website', key: 'website', ellipsis: true },
        { title: t('phone'), dataIndex: 'phone', key: 'phone', width: 140 },
        { title: t('address'), dataIndex: 'address', key: 'address', ellipsis: true },
        {
            title: t('actions'),
            key: 'actions',
            width: 130,
            render: (_: unknown, record: any) => (
                <div className="flex items-center text-gray-500">
                    <Tooltip title={tCommon('openDetails')}>
                        <Button
                            type="text"
                            shape="circle"
                            aria-label={tCommon('openDetails')}
                            icon={<ArrowRightOutlined />}
                            onClick={() => router.push(`/panel/providers/details/${record.id}`)}
                            style={{ color: '#2563EB' }}
                        />
                    </Tooltip>
                    {canDeleteProvider && (
                        <Tooltip title={tCommon('delete')}>
                            <div
                                role="button"
                                aria-label={tCommon('delete')}
                                className="w-10 h-10 flex items-center justify-center cursor-pointer hover:bg-gray-100 rounded-lg transition-colors"
                                onClick={() => handleDeleteClick(record)}
                            >
                                <Icon icon="trash" style={{ width: 17 }} />
                            </div>
                        </Tooltip>
                    )}
                    {canEditProvider && (
                        <Tooltip title={tCommon('edit')}>
                            <div
                                role="button"
                                aria-label={tCommon('edit')}
                                className="w-10 h-10 flex items-center justify-center cursor-pointer hover:bg-gray-100 rounded-lg transition-colors"
                                onClick={() => {
                                    setSelectedProvider(record);
                                    setEditModalOpen(true);
                                }}
                            >
                                <Icon icon="edit" style={{ width: 18 }} />
                            </div>
                        </Tooltip>
                    )}
                </div>
            ),
        },
    ];

    const handleApplyFilters = (values: Record<string, any>) => {
        setFilters(values);
        setPage(1);
    };

    return (
        <>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
                <div>
                    <Title level={4} style={{ margin: 0 }}>{t('providers')}</Title>
                    <Text type="secondary" style={{ fontSize: 13 }}>
                        {t('manageProviderDirectory')}
                    </Text>
                </div>
                <Space>
                    <Button onClick={() => openModal('providerFilter')}>
                        {t('filter')}
                    </Button>
                    {canCreateProvider && (
                        <Button type="primary" icon={<PlusOutlined />} onClick={() => openModal('addProvider')}>
                            {t('addProvider')}
                        </Button>
                    )}
                </Space>
            </div>

            <Card styles={{ body: { padding: 0 } }}>
                <Table
                    dataSource={providersData?.items ?? []}
                    columns={columns}
                    rowKey={(record) => String(record.id)}
                    loading={isLoading}
                    size="middle"
                    pagination={getTablePagination({
                        current: providersData?.page ?? page,
                        total: providersData?.total ?? 0,
                        pageSize: providersData?.pageSize ?? pageSize,
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
                                description={t('noProvidersYet')}
                            />
                        ),
                    }}
                />
            </Card>

            <ConfirmDeleteModalComponent
                open={deleteModalOpen}
                onCancel={() => setDeleteModalOpen(false)}
                onConfirm={handleConfirmDelete}
                title={t('deleteProvider')}
                description={
                    <>
                        {t('areYouSureYouWantToDeleteThisProvider')}{' '}
                        <strong>{selectedProvider?.name}</strong>?
                    </>
                }
            />

            {activeModal === 'addProvider'&&<ProvidersFormModalComponent
                open={activeModal === 'addProvider'}
                onClose={closeModal}
                isCreateForm
            />}
            {editModalOpen&&<ProvidersFormModalComponent
                open={editModalOpen}
                onClose={() => setEditModalOpen(false)}
                initialValues={selectedProvider}
            />}
            {activeModal==="providerFilter"&&<ProvidersFiltersModalComponent
                open={activeModal==="providerFilter"}
                onClose={() => closeModal()}
                onApply={handleApplyFilters}
                currentFilters={filters}
            />}
        </>
    );
}
