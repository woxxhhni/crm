'use client';

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Table, Button, App, Tag, Result, Card, Empty, Typography, Space, Tooltip } from 'antd';
import { ArrowRightOutlined, PlusOutlined } from '@ant-design/icons';
import { container } from '@/services/di-container';
import { EmployeesService } from '@/features/employees/services/employees.service';
import { useTranslate } from '@/locales/use-locales';
import { Icon } from '@/components/iconify/iconify.component';
import EmployeesFormModalComponent from './employees-form-modal.component';
import EmployeesDetailsComponent from './employees-details.component';
import EmployeesDeleteModalComponent from './employees-delete-modal.component';
import { useModalStore } from '@/store/store';
import { useRouter } from 'next/navigation';
import { paths } from '@/routes/paths';
import { CONFIG } from '../../../../global-config';
import { useState } from 'react';
import { usePermissionContext, PERMISSIONS } from '@/features/permissions';
import { getTablePagination } from '@/config/table-pagination';

const { Title, Text } = Typography;

export default function EmployeesComponent() {
  const { t } = useTranslate('employees');
  const { t: tCommon } = useTranslate('common');
  const router = useRouter();
  const { notification } = App.useApp();
  const queryClient = useQueryClient();
  const service = container.resolve(EmployeesService);
  const { activeModal, openModal, closeModal } = useModalStore();
  const { hasPermission, loading: permissionLoading } = usePermissionContext();

  // Permission checks
  const canViewEmployees = hasPermission(PERMISSIONS.EMPLOYEE_VIEW);
  const canViewEmployeeDetail = hasPermission(PERMISSIONS.EMPLOYEE_VIEW_DETAIL);
  const canCreateEmployee = hasPermission(PERMISSIONS.EMPLOYEE_CREATE);
  const canEditEmployee = hasPermission(PERMISSIONS.EMPLOYEE_EDIT);
  const canDeleteEmployee = hasPermission(PERMISSIONS.EMPLOYEE_DELETE);

  const [pagination, setPagination] = useState({ page: 1, pageSize: 10 });
  const [editingEmployee, setEditingEmployee] = useState<any>(null);
  const [deletingEmployee, setDeletingEmployee] = useState<any>(null);

  const { data: employeesData, isLoading } = useQuery({
    queryKey: ['employees-list', pagination],
    queryFn: () => service.list(pagination.page, pagination.pageSize),
    enabled: canViewEmployees,
  });

  const deleteMutation = useMutation({
    mutationFn: async (id: number) => {
      await service.delete(id);
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['employees-list'] });
      notification.success({ message: t('employeeDeletedSuccessfully') });
      setDeletingEmployee(null);
    },
  });

  const handleEditEmployee = (employee: any) => {
    setEditingEmployee(employee);
    openModal('addEmployee');
  };

  const handleCloseForm = () => {
    closeModal();
    setEditingEmployee(null);
  };

  const handleDelete = (employee: any) => {
    setDeletingEmployee(employee);
  };

  // Access denied for users without view permission
  if (!permissionLoading && !canViewEmployees) {
    return (
      <Result
        status="403"
        title={tCommon('accessDenied')}
        subTitle={tCommon('noPermissionToViewPage')}
        extra={
          <Button type="primary" onClick={() => router.push(CONFIG.auth.redirectPath)}>
            {tCommon('backToHome')}
          </Button>
        }
      />
    );
  }

  const getRoleLabel = (role: string) => {
    const roleLabelMap: Record<string, string> = {
      admin: t('admin'),
      manager: t('manager'),
      employee: t('employee'),
    };

    return roleLabelMap[role?.toLowerCase()] || role;
  };

  const columns = [
    { title: t('id'), dataIndex: 'id', key: 'id', width: 70 },
    {
      title: t('name'),
      dataIndex: 'name',
      key: 'name',
      ellipsis: true,
    },
    {
      title: t('role'),
      dataIndex: 'role',
      key: 'role',
      width: 110,
      render: (role: string) => {
        const roleColorMap: Record<string, string> = {
          admin: 'red',
          manager: 'blue',
          employee: 'default',
        };
        return <Tag color={roleColorMap[role?.toLowerCase()] || 'default'}>{getRoleLabel(role)}</Tag>;
      },
    },
    {
      title: t('email'),
      dataIndex: 'email',
      key: 'email',
      ellipsis: true,
    },
    {
      title: t('phone'),
      dataIndex: 'phone',
      key: 'phone',
      width: 140,
    },
    {
      title: t('address'),
      dataIndex: 'address',
      key: 'address',
      ellipsis: true,
    },
    {
      title: t('status'),
      dataIndex: 'isActive',
      key: 'isActive',
      width: 100,
      render: (isActive: boolean) => <Tag color={isActive ? 'success' : 'error'}>{isActive ? t('active') : t('deactive')}</Tag>,
    },
    {
      title: t('actions'),
      key: 'actions',
      width: 130,
      render: (_: any, record: any) => (
        <div className='flex items-center text-gray-500'>
          {canViewEmployeeDetail && (
            <Tooltip title={tCommon('openDetails')}>
              <Button
                type='text'
                shape='circle'
                aria-label={tCommon('openDetails')}
                icon={<ArrowRightOutlined />}
                onClick={() => router.push(paths.panel.employeeDetails(record?.id))}
                style={{ color: '#2563EB' }}
              />
            </Tooltip>
          )}
          {canDeleteEmployee && (
            <Tooltip title={tCommon('delete')}>
              <div
                role='button'
                aria-label={tCommon('delete')}
                className='w-10 h-10 flex items-center justify-center cursor-pointer hover:bg-gray-100 rounded-lg transition-colors'
                onClick={() => handleDelete(record)}
              >
                <Icon icon='trash' style={{ width: 17 }} />
              </div>
            </Tooltip>
          )}
          {canEditEmployee && (
            <Tooltip title={tCommon('edit')}>
              <div
                role='button'
                aria-label={tCommon('edit')}
                className='w-10 h-10 flex items-center justify-center cursor-pointer hover:bg-gray-100 rounded-lg transition-colors'
                onClick={() => handleEditEmployee(record)}
              >
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
          <Title level={4} style={{ margin: 0 }}>{t('employees')}</Title>
          <Text type="secondary" style={{ fontSize: 13 }}>
            {t('manageEmployeesDirectory')}
          </Text>
        </div>
        <Space>
          {canCreateEmployee && (
            <Button type="primary" icon={<PlusOutlined />} onClick={() => openModal('addEmployee')}>
              {t('addEmployee')}
            </Button>
          )}
        </Space>
      </div>

      <Card styles={{ body: { padding: 0 } }}>
        <Table
          columns={columns}
          dataSource={employeesData?.items || []}
          rowKey='id'
          loading={isLoading}
          size="middle"
          pagination={getTablePagination({
            current: pagination.page,
            total: employeesData?.total || 0,
            pageSize: pagination.pageSize,
            onChange: (page, pageSize) => setPagination({ page, pageSize }),
          })}
          scroll={{ x: 'max-content' }}
          locale={{
            emptyText: (
              <Empty
                image={Empty.PRESENTED_IMAGE_SIMPLE}
                description={t('noEmployeesYet')}
              />
            ),
          }}
        />
      </Card>

      {activeModal === 'addEmployee' && (canCreateEmployee || (canEditEmployee && editingEmployee)) && (
        <EmployeesFormModalComponent open={true} onClose={handleCloseForm} initialValues={editingEmployee} isCreateForm={!editingEmployee} />
      )}

      {deletingEmployee && canDeleteEmployee && (
        <EmployeesDeleteModalComponent
          employee={deletingEmployee}
          onConfirm={() => deleteMutation.mutate(deletingEmployee.id)}
          onCancel={() => setDeletingEmployee(null)}
          isLoading={deleteMutation.isPending}
        />
      )}
    </>
  );
}
