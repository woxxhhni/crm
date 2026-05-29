'use client';

import 'reflect-metadata';
import { useState } from 'react';
import { Table, App, Card, Empty, Typography, Space, Button, Tooltip } from 'antd';
import { ArrowRightOutlined, PlusOutlined } from '@ant-design/icons';
import { useRouter } from 'next/navigation';
import { Icon } from '@/components/iconify/iconify.component';
import { useTranslate } from '@/locales/use-locales';
import { useModalStore } from '@/store/store';
import ClientsFormModalComponent from '@/features/clients/components/clients-form-modal.component';
import { ConfirmDeleteModalComponent } from '@/components/modal/confirm-delete-modal.component';
import ClientsFilterModalComponent from '@/features/clients/components/clients-filters-modal.component';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { container } from 'tsyringe';
import { ClientsService } from '@/features/clients/services/clients.service';
import { ClientRecord } from '@/api/clients/types';
import { usePermissionContext, PERMISSIONS } from '@/features/permissions';
import { getTablePagination } from '@/config/table-pagination';

const { Title, Text } = Typography;

const ClientsComponent = () => {
    const [deleteModalOpen, setDeleteModalOpen] = useState(false);
    const [editModalOpen, setEditModalOpen] = useState(false);
    const [selectedClient, setSelectedClient] = useState<ClientRecord | null>(null);

    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(10);
    const [filters, setFilters] = useState<Record<string, any>>({});

    const { notification } = App.useApp();
    const { t } = useTranslate('clients');
    const { t: tCommon } = useTranslate('common');
    const { activeModal, openModal, closeModal } = useModalStore();
    const router = useRouter();
    const queryClient = useQueryClient();
    const service = container.resolve(ClientsService);

    const { hasPermission } = usePermissionContext();
    const canCreateClient = hasPermission(PERMISSIONS.CLIENT_CREATE);
    const canEditClient = hasPermission(PERMISSIONS.CLIENT_EDIT);
    const canDeleteClient = hasPermission(PERMISSIONS.CLIENT_DELETE);

    const { data: clientsData, isLoading } = useQuery({
        queryKey: ['clients-list', page, pageSize, filters],
        queryFn: async () => await service.list(page, pageSize, filters),
    });

    const deleteMutation = useMutation({
        mutationFn: async (id: number) => service.delete(id),
        onSuccess: async () => {
            await queryClient.invalidateQueries({ queryKey: ['clients-list'] });
            notification.success({ message: t('clientDeletedSuccessfully') });
        },
    });

    const handleDeleteClick = (record: ClientRecord) => {
        setSelectedClient(record);
        setDeleteModalOpen(true);
    };

    const handleConfirmDelete = async () => {
        if (!selectedClient) return;
        setDeleteModalOpen(false);
        deleteMutation.mutate(Number(selectedClient.id));
    };

    const handleApplyFilters = (values: Record<string, any>) => {
        setFilters(values);
        setPage(1);
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
            render: (_: unknown, record: ClientRecord) => (
                <div className="flex items-center text-gray-500">
                    <Tooltip title={tCommon('openDetails')}>
                        <Button
                            type="text"
                            shape="circle"
                            aria-label={tCommon('openDetails')}
                            icon={<ArrowRightOutlined />}
                            onClick={() => router.push(`/panel/clients/details/${record.id}`)}
                            style={{ color: '#2563EB' }}
                        />
                    </Tooltip>
                    {canDeleteClient && (
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
                    {canEditClient && (
                        <Tooltip title={tCommon('edit')}>
                            <div
                                role="button"
                                aria-label={tCommon('edit')}
                                className="w-10 h-10 flex items-center justify-center cursor-pointer hover:bg-gray-100 rounded-lg transition-colors"
                                onClick={() => {
                                    setSelectedClient(record);
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

    return (
        <>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
                <div>
                    <Title level={4} style={{ margin: 0 }}>{t('clients')}</Title>
                    <Text type="secondary" style={{ fontSize: 13 }}>
                        {t('manageClientDirectory')}
                    </Text>
                </div>
                <Space>
                    <Button onClick={() => openModal('clientFilter')}>
                        {t('filter')}
                    </Button>
                    {canCreateClient && (
                        <Button type="primary" icon={<PlusOutlined />} onClick={() => openModal('addClient')}>
                            {t('addClient')}
                        </Button>
                    )}
                </Space>
            </div>

            <Card styles={{ body: { padding: 0 } }}>
                <Table
                    dataSource={clientsData?.items ?? []}
                    columns={columns}
                    rowKey={(record) => String(record.id)}
                    loading={isLoading}
                    size="middle"
                    pagination={getTablePagination({
                        current: clientsData?.page ?? page,
                        total: clientsData?.total ?? 0,
                        pageSize: clientsData?.pageSize ?? pageSize,
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
                                description={t('noClientsYet')}
                            />
                        ),
                    }}
                />
            </Card>

            <ConfirmDeleteModalComponent
                open={deleteModalOpen}
                onCancel={() => setDeleteModalOpen(false)}
                onConfirm={handleConfirmDelete}
                title={t('deleteClient')}
                description={
                    <>
                        {t('areYouSureYouWantToDeleteThisClient')} <strong>{selectedClient?.name}</strong>?
                    </>
                }
            />

            {activeModal === 'addClient' && (
                <ClientsFormModalComponent open onClose={closeModal} isCreateForm />
            )}
            {editModalOpen && (
                <ClientsFormModalComponent
                    open
                    onClose={() => setEditModalOpen(false)}
                    initialValues={selectedClient}
                />
            )}

            {activeModal==="clientFilter"&&<ClientsFilterModalComponent
                open={activeModal==="clientFilter"}
                onClose={() => closeModal()}
                onApply={handleApplyFilters}
                currentFilters={filters}
            />}
        </>
    );
};

export default ClientsComponent;
