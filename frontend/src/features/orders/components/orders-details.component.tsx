'use client';
import 'reflect-metadata';
import { Avatar, Button, Card, Divider, Space, Typography, Tabs, Tag, Row, Col, Modal, Select, message, App, Empty, Tooltip } from 'antd';
import {
  ArrowRightOutlined, EditOutlined, DeleteOutlined, UserOutlined,
  ClockCircleOutlined, DollarOutlined, FileTextOutlined,
  MailOutlined, ShoppingOutlined, TeamOutlined, PlusOutlined,
  CheckCircleOutlined, CloseCircleOutlined, PauseCircleOutlined,
  PlayCircleOutlined,
} from '@ant-design/icons';
import LoadingSpinnerComponent from '@/components/loading/loading-spinner.component';
import { Icon } from '@/components/iconify/iconify.component';
import { FilePreviewCard } from '@/components/file-preview/file-preview-card.component';
import { useTranslate } from '@/locales/use-locales';
import { useParams, useRouter } from 'next/navigation';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { container } from 'tsyringe';
import { OrdersService } from '@/features/orders/services/orders.service';
import { useState, useMemo } from 'react';
import { ConfirmDeleteModalComponent } from '@/components/modal/confirm-delete-modal.component';
import { paths } from '@/routes/paths';
import AccountingTabComponent from './accounting-tab.component';
import ActivityLogsTabComponent from './activity-logs-tab.component';
import dayjs from 'dayjs';
import relativeTime from 'dayjs/plugin/relativeTime';
import { usePermissionContext, PERMISSIONS } from '@/features/permissions';
import { defaultSelectFilter } from '@/utils/select-utils';
import ExtraProvidersTabComponent from './extra-providers-tab.component';
import OrdersFormModalComponent from './orders-form-modal.component';
import OrderActionModalComponent from './order-action-modal.component';
import { translateWorkflowLabel } from '../utils/workflow-labels';

dayjs.extend(relativeTime);

const { Title, Text, Paragraph } = Typography;

/* ─── Status config ─── */
const STATUS_MAP: Record<string, { color: string; bg: string; border: string }> = {
  inprogress: { color: '#2563EB', bg: '#EFF6FF', border: '#BFDBFE' },
  completed:  { color: '#059669', bg: '#ECFDF5', border: '#A7F3D0' },
  suspended:  { color: '#D97706', bg: '#FFFBEB', border: '#FDE68A' },
  canceled:   { color: '#DC2626', bg: '#FEF2F2', border: '#FECACA' },
};

function getStatusStyle(status: string) {
  return STATUS_MAP[status?.toLowerCase()] || { color: '#64748B', bg: '#F1F5F9', border: '#E2E8F0' };
}

/* ─── Info row helper (like client/provider detail pages) ─── */
function InfoRow({ icon, label, value }: { icon: React.ReactNode; label: string; value?: string | null }) {
  return (
    <div style={{ display: 'flex', alignItems: 'flex-start', gap: 12, padding: '12px 0' }}>
      <span style={{ color: '#64748B', fontSize: 16, marginTop: 2 }}>{icon}</span>
      <div style={{ minWidth: 120 }}>
        <Text style={{ color: '#94A3B8', fontSize: 13, display: 'block' }}>{label}</Text>
        <Text style={{ fontSize: 14, color: value ? '#0F172A' : '#CBD5E1', fontWeight: 500 }}>
          {value || '—'}
        </Text>
      </div>
    </div>
  );
}

export default function OrdersDetailsComponent() {
  const { t } = useTranslate('orders');
  const { t: tCommon } = useTranslate('common');
  const { id } = useParams();
  const orderId = typeof id === 'string' ? id : '';
  const service = container.resolve(OrdersService);
  const queryClient = useQueryClient();
  const router = useRouter();
  const { hasPermission } = usePermissionContext();

  // Permission checks
  const canViewAssignedEmployees = hasPermission(PERMISSIONS.ORDER_VIEW_ASSIGNED_EMPLOYEES);
  const canAssignEmployee = hasPermission(PERMISSIONS.ORDER_ASSIGN_EMPLOYEE);
  const canRemoveAssignedEmployee = hasPermission(PERMISSIONS.ORDER_REMOVE_ASSIGNED_EMPLOYEE);
  const canViewClientPayments = hasPermission(PERMISSIONS.ORDER_VIEW_CLIENT_PAYMENTS);
  const canViewProviderPayments = hasPermission(PERMISSIONS.ORDER_VIEW_PROVIDER_PAYMENTS);
  const canEditOrder = hasPermission(PERMISSIONS.ORDER_EDIT);
  const canDeleteOrder = hasPermission(PERMISSIONS.ORDER_DELETE);
  const canChangeOrderStatus = hasPermission(PERMISSIONS.ORDER_CHANGE_STATUS);

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedEmployee, setSelectedEmployee] = useState<number | null>(null);
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [employeeToDelete, setEmployeeToDelete] = useState<number | null>(null);
  const [activeTab, setActiveTab] = useState('1');
  const [editModalOpen, setEditModalOpen] = useState(false);
  const [deleteOrderModalOpen, setDeleteOrderModalOpen] = useState(false);
  const [orderActionModalOpen, setOrderActionModalOpen] = useState(false);
  const [orderActionType, setOrderActionType] = useState<'complete' | 'cancel' | 'suspend' | 'unsuspend' | null>(null);
  const [updatingStageId, setUpdatingStageId] = useState<number | null>(null);

  const { notification } = App.useApp();

  const {
    data: orderDetails,
    isLoading,
    isError,
    refetch: refetchOrderDetails,
  } = useQuery({
    queryKey: ['order-edit-details', orderId],
    queryFn: () => service.getDetails(orderId),
    enabled: !!orderId,
  });

  const { data: employeesData, isLoading: empOptionsLoading } = useQuery({
    queryKey: ['employees-option-list'],
    queryFn: () => service.employeeOptions(),
    enabled: canAssignEmployee && (activeTab === '1' || (activeTab === '4' && isModalOpen)),
  });

  const { mutateAsync, isPending } = useMutation({
    mutationKey: ['assign-employee', orderId],
    mutationFn: (payload: any) => service.assignEmployee(orderId, payload),
    onSuccess: async () => {
      notification.success({ message: t('employeeAssigned') });
      setIsModalOpen(false);
      setSelectedEmployee(null);
      await queryClient.invalidateQueries({ queryKey: ['order-edit-details', orderId] });
    },
    onError: () => {
      message.error(t('failedToAssignEmployee'));
    },
  });

  const deleteMutation = useMutation({
    mutationFn: async () => {
      await service.deleteEmployee(orderId, employeeToDelete as number | string);
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['order-edit-details', orderId] });
      notification.success({ message: t('employeeDeletedSuccessfully') });
      setDeleteModalOpen(false);
    },
  });

  const stageAssigneeMutation = useMutation({
    mutationFn: async ({ stageId, employeeId }: { stageId: number; employeeId: number | null }) => {
      setUpdatingStageId(stageId);
      await service.setStageAssignee(orderId, stageId, employeeId);
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['order-edit-details', orderId] });
      notification.success({ message: t('stageAssigneeUpdated') });
    },
    onError: () => {
      notification.error({ message: t('failedToUpdateStageAssignee') });
    },
    onSettled: () => {
      setUpdatingStageId(null);
    },
  });

  // Delete order mutation
  const deleteOrderMutation = useMutation({
    mutationFn: async () => {
      await service.deleteOrder(orderId);
    },
    onSuccess: async () => {
      notification.success({ message: t('orderDeletedSuccessfully') });
      await queryClient.invalidateQueries({ queryKey: ['orders-list'] });
      router.push(paths.panel.orders);
    },
    onError: () => {
      notification.error({ message: t('failedToDeleteOrder') });
    },
  });

  // Order action handler
  const handleOrderAction = (actionType: 'complete' | 'cancel' | 'suspend' | 'unsuspend') => {
    setOrderActionType(actionType);
    setOrderActionModalOpen(true);
  };

  const handleAssignEmployee = async () => {
    if (!selectedEmployee) {
      message.warning(t('pleaseSelectEmployee'));
      return;
    }

    const payload = {
      userIds: [selectedEmployee],
      assignedByUserId: 1,
    };
    await mutateAsync(payload);
  };

  const handleTabChange = (key: string) => {
    const previousTab = activeTab;
    setActiveTab(key);

    // Only refetch when actually switching tabs (not on initial render)
    if (previousTab === key) return;

    // Refetch data when switching tabs
    if (key === '1' || key === '4') {
      refetchOrderDetails();
    } else if (key === '2') {
      queryClient.invalidateQueries({ queryKey: ['client-payments', orderId] });
    } else if (key === '3') {
      queryClient.invalidateQueries({ queryKey: ['provider-payments', orderId] });
    }
  };

  const handleStageAssigneeChange = (stageId: number, employeeId: number | null) => {
    stageAssigneeMutation.mutate({ stageId, employeeId });
  };

  if (isLoading) return <LoadingSpinnerComponent />;

  if (isError || !orderDetails) {
    return <Text type='danger'>{t('failedToLoadOrder')}</Text>;
  }

  const statusKey = orderDetails?.status?.toLowerCase() || 'default';
  const statusStyle = getStatusStyle(statusKey);

  const orderStatusLabel: Record<string, string> = {
    inprogress: t('inProgress'),
    completed: t('completed'),
    canceled: t('canceled'),
    suspended: t('suspended'),
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 24 }}>
      {/* ── Order Info Card ── */}
      <Card
        styles={{ body: { padding: 0 } }}
        style={{ borderRadius: 16, overflow: 'hidden', border: '1px solid #E2E8F0' }}
      >
        {/* Header area with order number + status + actions */}
        <div style={{ padding: '24px 32px 20px', borderBottom: '1px solid #F1F5F9' }}>
          {/* Top row: Order info + status badge */}
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 14 }}>
              <div
                style={{
                  width: 48, height: 48, borderRadius: 12,
                  background: '#EFF6FF',
                  display: 'flex', alignItems: 'center', justifyContent: 'center',
                }}
              >
                <ShoppingOutlined style={{ color: '#2563EB', fontSize: 22 }} />
              </div>
              <div>
                <Title level={4} style={{ margin: 0, color: '#0F172A' }}>
                  {orderDetails?.orderNumber}
                </Title>
                <Text style={{ color: '#94A3B8', fontSize: 13 }}>
                  {orderDetails?.title}
                </Text>
              </div>
            </div>
            <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
              <Tag
                style={{
                  borderRadius: 8,
                  padding: '4px 16px',
                  fontWeight: 600,
                  fontSize: 13,
                  border: `1px solid ${statusStyle.border}`,
                  background: statusStyle.bg,
                  color: statusStyle.color,
                  margin: 0,
                }}
              >
                {orderStatusLabel[statusKey] ?? orderDetails?.status}
              </Tag>
              {orderDetails?.updatedAt && (
                <Text style={{ color: '#CBD5E1', fontSize: 12 }}>
                  <ClockCircleOutlined style={{ marginRight: 4 }} />
                  {dayjs(orderDetails.updatedAt).fromNow()}
                </Text>
              )}
            </div>
          </div>

          {/* Action buttons row — only show if any actions are available */}
          {(canEditOrder || canDeleteOrder || canChangeOrderStatus) && (
            <div style={{
              display: 'flex', justifyContent: 'space-between', alignItems: 'center',
              marginTop: 16, paddingTop: 16, borderTop: '1px solid #F1F5F9',
              flexWrap: 'wrap', gap: 10,
            }}>
              {/* LEFT: CRUD Actions (Edit + Delete) */}
              <div style={{ display: 'flex', gap: 10 }}>
                {canEditOrder && statusKey === 'inprogress' && (
                  <Button
                    icon={<EditOutlined />}
                    onClick={() => setEditModalOpen(true)}
                    style={{
                      borderRadius: 10, height: 38, paddingInline: 18, fontWeight: 500,
                      borderColor: '#BFDBFE', color: '#2563EB', background: '#EFF6FF',
                      fontSize: 13,
                    }}
                  >
                    {t('editOrder')}
                  </Button>
                )}
                {canDeleteOrder && (
                  <Button
                    icon={<DeleteOutlined />}
                    onClick={() => setDeleteOrderModalOpen(true)}
                    style={{
                      borderRadius: 10, height: 38, paddingInline: 18, fontWeight: 500,
                      borderColor: '#FECACA', color: '#DC2626', background: '#FEF2F2',
                      fontSize: 13,
                    }}
                  >
                    {t('deleteOrder')}
                  </Button>
                )}
              </div>

              {/* RIGHT: Status Workflow Actions */}
              <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
                {canChangeOrderStatus && statusKey === 'inprogress' && (
                  <>
                    <Button
                      type="primary"
                      icon={<CheckCircleOutlined />}
                      onClick={() => handleOrderAction('complete')}
                      style={{
                        borderRadius: 10, height: 38, paddingInline: 18, fontWeight: 600,
                        background: '#059669', borderColor: '#059669',
                        fontSize: 13,
                      }}
                    >
                      {tCommon('complete')}
                    </Button>
                    <Button
                      icon={<PauseCircleOutlined />}
                      onClick={() => handleOrderAction('suspend')}
                      style={{
                        borderRadius: 10, height: 38, paddingInline: 18, fontWeight: 500,
                        borderColor: '#FDE68A', color: '#B45309', background: '#FFFBEB',
                        fontSize: 13,
                      }}
                    >
                      {tCommon('suspend')}
                    </Button>
                    <Button
                      icon={<CloseCircleOutlined />}
                      onClick={() => handleOrderAction('cancel')}
                      style={{
                        borderRadius: 10, height: 38, paddingInline: 18, fontWeight: 500,
                        borderColor: '#FECACA', color: '#DC2626', background: '#FEF2F2',
                        fontSize: 13,
                      }}
                    >
                      {tCommon('cancel')}
                    </Button>
                  </>
                )}

                {canChangeOrderStatus && statusKey === 'suspended' && (
                  <Button
                    type="primary"
                    icon={<PlayCircleOutlined />}
                    onClick={() => handleOrderAction('unsuspend')}
                    style={{
                      borderRadius: 10, height: 38, paddingInline: 18, fontWeight: 600,
                      background: '#2563EB', borderColor: '#2563EB',
                      fontSize: 13,
                    }}
                  >
                    {tCommon('unsuspend')}
                  </Button>
                )}
              </div>
            </div>
          )}
        </div>

        {/* Key metrics row */}
        <div style={{ padding: '20px 32px' }}>
          <Row gutter={16} style={{ marginBottom: 20 }}>
            <Col xs={24} sm={8}>
              <div style={{
                background: '#F8FAFC', borderRadius: 12, padding: '16px 20px',
                border: '1px solid #F1F5F9',
              }}>
                <Text style={{ color: '#94A3B8', fontSize: 12, display: 'block', marginBottom: 4 }}>{t('currentStage')}</Text>
                <Text style={{ fontSize: 15, fontWeight: 600, color: '#0F172A' }}>
                  {translateWorkflowLabel(orderDetails?.currentStageName, t) || '—'}
                </Text>
                {orderDetails?.currentStepName && (
                  <Text style={{ fontSize: 12, color: '#64748B', display: 'block', marginTop: 2 }}>
                    {translateWorkflowLabel(orderDetails.currentStepName, t)}
                  </Text>
                )}
              </div>
            </Col>
            <Col xs={24} sm={8}>
              <div style={{
                background: '#F8FAFC', borderRadius: 12, padding: '16px 20px',
                border: '1px solid #F1F5F9',
              }}>
                <Text style={{ color: '#94A3B8', fontSize: 12, display: 'block', marginBottom: 4 }}>{t('amount')}</Text>
                <Text style={{ fontSize: 15, fontWeight: 700, color: '#0F172A' }}>
                  {new Intl.NumberFormat('en-US', {
                    style: 'currency',
                    currency: orderDetails?.sellCurrency || 'AFN',
                    minimumFractionDigits: 2,
                    maximumFractionDigits: 2,
                  }).format(orderDetails?.sellAmount || 0)}
                </Text>
              </div>
            </Col>
            <Col xs={24} sm={8}>
              <div style={{
                background: '#F8FAFC', borderRadius: 12, padding: '16px 20px',
                border: '1px solid #F1F5F9',
              }}>
                <Text style={{ color: '#94A3B8', fontSize: 12, display: 'block', marginBottom: 4 }}>{t('client')}</Text>
                <Text style={{ fontSize: 15, fontWeight: 600, color: '#0F172A' }}>
                  {orderDetails?.clientName ?? '—'}
                </Text>
              </div>
            </Col>
          </Row>

          {/* Details grid with icons */}
          <Row gutter={[48, 0]}>
            <Col xs={24} md={12}>
              <InfoRow icon={<MailOutlined />} label={t('email')} value={orderDetails?.clientEmail} />
              {orderDetails?.description && (
                <InfoRow icon={<FileTextOutlined />} label={t('description')} value={orderDetails.description} />
              )}
              <div style={{ paddingTop: 12 }}>
                <Text style={{ color: '#94A3B8', fontSize: 13, display: 'block', marginBottom: 8 }}>
                  <FileTextOutlined style={{ marginRight: 6 }} />{t('buyerInvoice')}
                </Text>
                {orderDetails?.buyInvoiceLinks?.length ? (
                  <div style={{ display: 'flex', flexWrap: 'wrap', gap: 8 }}>
                    {orderDetails.buyInvoiceLinks.map((file: { id: number | string; fileId?: number; fileName: string; url: string }) => (
                      <FilePreviewCard key={String(file.id)} file={{ id: typeof file.id === 'number' ? file.id : undefined, fileId: file.fileId, fileName: file.fileName, url: file.url }} />
                    ))}
                  </div>
                ) : (
                  <Text type='secondary'>{t('noFiles')}</Text>
                )}
              </div>
            </Col>
            <Col xs={24} md={12}>
              <div style={{ paddingTop: 12 }}>
                <Text style={{ color: '#94A3B8', fontSize: 13, display: 'block', marginBottom: 8 }}>
                  <FileTextOutlined style={{ marginRight: 6 }} />{t('sellerInvoice')}
                </Text>
                {orderDetails?.sellInvoiceLinks?.length ? (
                  <div style={{ display: 'flex', flexWrap: 'wrap', gap: 8 }}>
                    {orderDetails.sellInvoiceLinks.map((file: { id: number | string; fileId?: number; fileName: string; url: string }) => (
                      <FilePreviewCard key={String(file.id)} file={{ id: typeof file.id === 'number' ? file.id : undefined, fileId: file.fileId, fileName: file.fileName, url: file.url }} />
                    ))}
                  </div>
                ) : (
                  <Text type='secondary'>{t('noFiles')}</Text>
                )}
              </div>
            </Col>
          </Row>
        </div>
      </Card>

      {/* ── Tabs Card ── */}
      <Card
        style={{ borderRadius: 16, border: '1px solid #E2E8F0' }}
        styles={{ body: { padding: '0 24px 24px' } }}
      >
        <Tabs
          activeKey={activeTab}
          onChange={handleTabChange}
          style={{ marginTop: 8 }}
          items={[
            {
              key: '1',
              label: t('activityLogs'),
              children: activeTab === '1' ? (
                <ActivityLogsTabComponent
                  key={`activity-logs-${orderDetails?.updatedAt || Date.now()}`}
                  steps={orderDetails?.steps || []}
                  orderId={orderId}
                  orderStatus={orderDetails?.status}
                  currentStepId={orderDetails?.currentStepId}
                  currentStepName={orderDetails?.currentStepName}
                  currentStageName={orderDetails?.currentStageName}
                  stageAssignments={orderDetails?.stageAssignments || []}
                  employeeOptions={employeesData || []}
                  employeeOptionsLoading={empOptionsLoading}
                  canAssignStageAssignee={canAssignEmployee && statusKey === 'inprogress'}
                  updatingStageId={updatingStageId}
                  onStageAssigneeChange={handleStageAssigneeChange}
                />
              ) : null,
            },
            // Client accounting tab - only visible if user has permission
            ...(canViewClientPayments ? [{
              key: '2',
              label: t('clientAccounting'),
              children: activeTab === '2' ? <AccountingTabComponent orderId={orderId} type='client' orderStatus={orderDetails?.status} /> : null,
            }] : []),
            // Provider accounting tab - only visible if user has permission
            ...(canViewProviderPayments ? [{
              key: '3',
              label: t('providerAccounting'),
              children: activeTab === '3' ? <AccountingTabComponent orderId={orderId} type='provider' orderStatus={orderDetails?.status} /> : null,
            }] : []),
            // Extra Providers tab
            {
              key: '5',
              label: t('extraProviders'),
              children: activeTab === '5' ? (
                <ExtraProvidersTabComponent
                  orderId={orderId}
                  extraProviders={orderDetails?.extraProviders || []}
                  loading={isLoading}
                />
              ) : null,
            },
            // Assigned employees tab - only visible if user has permission
            ...(canViewAssignedEmployees ? [{
              key: '4',
              label: t('assignedEmployee'),
              children:
                activeTab === '4' ? (
                  <>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
                      <Title level={5} style={{ margin: 0 }}>{t('employees')}</Title>
                      {canAssignEmployee && orderDetails?.status?.toLowerCase() === 'inprogress' && (
                        <Button
                          type='primary'
                          icon={<PlusOutlined />}
                          onClick={() => setIsModalOpen(true)}
                          style={{ borderRadius: 10, height: 40, paddingInline: 20 }}
                        >
                          {tCommon('addEmployee')}
                        </Button>
                      )}
                    </div>

                    {orderDetails?.employees?.length ? (
                      <div style={{ display: 'flex', flexDirection: 'column', gap: 10, padding: 16, borderRadius: 12, border: '1px solid #E2E8F0' }}>
                        {orderDetails?.employees?.map((emp: any) => (
                          <div
                            key={emp?.id}
                            style={{
                              display: 'flex', justifyContent: 'space-between', alignItems: 'center',
                              padding: '12px 16px', border: '1px solid #F1F5F9', borderRadius: 10, background: '#FAFBFC',
                            }}
                          >
                            <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
                              <Avatar icon={<UserOutlined />} style={{ backgroundColor: '#3B82F6' }} />
                              <Text style={{ fontWeight: 500 }}>{emp?.name}</Text>
                            </div>
                            <Space size='small'>
                              <Tooltip title={tCommon('openDetails')}>
                                <Button
                                  type='text'
                                  shape='circle'
                                  aria-label={tCommon('openDetails')}
                                  onClick={() => router.push(paths.panel.employeeDetails(emp?.employeeId))}
                                  icon={<ArrowRightOutlined />}
                                  style={{ color: '#2563EB' }}
                                />
                              </Tooltip>
                              {canRemoveAssignedEmployee && (
                                <Tooltip title={tCommon('delete')}>
                                  <Button
                                    type='text'
                                    shape='circle'
                                    aria-label={tCommon('delete')}
                                    icon={<DeleteOutlined />}
                                    style={{ color: '#EF4444' }}
                                    onClick={() => {
                                      setEmployeeToDelete(emp.employeeId);
                                      setDeleteModalOpen(true);
                                    }}
                                  />
                                </Tooltip>
                              )}
                            </Space>
                          </div>
                        ))}
                      </div>
                    ) : (
                      <div style={{ padding: 40, borderRadius: 12, border: '1px solid #E2E8F0', background: '#FAFBFC' }}>
                        <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description={<Text style={{ color: '#94A3B8' }}>{t('noEmployeesYet')}</Text>} />
                      </div>
                    )}

                    {canAssignEmployee && (
                      <Modal
                        open={isModalOpen}
                        title={t('assignEmployee')}
                        onCancel={() => setIsModalOpen(false)}
                        onOk={handleAssignEmployee}
                        okText={t('confirm')}
                        cancelText={t('cancel')}
                        confirmLoading={isPending}>
                        <Select
                          style={{ width: '100%' }}
                          showSearch
                          filterOption={defaultSelectFilter}
                          placeholder={t('selectEmployee')}
                          value={selectedEmployee}
                          loading={empOptionsLoading}
                          onChange={(value) => setSelectedEmployee(value)}
                          options={employeesData}
                        />
                      </Modal>
                    )}
                  </>
                ) : null,
            }] : []),
          ]}
        />
      </Card>
      <ConfirmDeleteModalComponent
        open={deleteModalOpen}
        onCancel={() => setDeleteModalOpen(false)}
        onConfirm={() => deleteMutation?.mutateAsync()}
        title={t('deleteEmployee')}
        description={<>{t('areYouSureYouWantToDeleteThisEmployee')}</>}
        loading={deleteMutation.isPending}
      />

      {/* Delete Order Modal */}
      <ConfirmDeleteModalComponent
        open={deleteOrderModalOpen}
        onCancel={() => setDeleteOrderModalOpen(false)}
        onConfirm={() => deleteOrderMutation.mutateAsync()}
        title={t('deleteOrder')}
        description={<>{t('areYouSureYouWantToDeleteThisOrder')}</>}
        loading={deleteOrderMutation.isPending}
      />

      {/* Edit Order Modal */}
      {editModalOpen && canEditOrder && (
        <OrdersFormModalComponent
          open={editModalOpen}
          onClose={() => {
            setEditModalOpen(false);
            refetchOrderDetails();
          }}
          initialValues={{ id: orderId }}
        />
      )}

      {/* Order Action Modal */}
      <OrderActionModalComponent
        open={orderActionModalOpen}
        onClose={() => {
          setOrderActionModalOpen(false);
          setOrderActionType(null);
        }}
        orderId={orderId}
        actionType={orderActionType}
      />
    </div>
  );
}
