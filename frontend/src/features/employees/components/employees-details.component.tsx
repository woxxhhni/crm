'use client';

import { useState } from 'react';
import { Avatar, Button, Tag, Card, Typography, App, Result, Row, Col, Space } from 'antd';
import {
  PhoneOutlined,
  MailOutlined,
  EnvironmentOutlined,
  FileTextOutlined,
  EditOutlined,
  LockOutlined,
  UserOutlined,
  SafetyOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
} from '@ant-design/icons';
import { container } from '@/services/di-container';
import { EmployeesService } from '@/features/employees/services/employees.service';
import { useTranslate } from '@/locales/use-locales';
import { useQuery } from '@tanstack/react-query';
import EmployeesResetPasswordModalComponent from './employees-reset-password-modal.component';
import { useParams, useRouter } from "next/navigation";
import {useModalStore} from "@/store/store";
import EmployeesFormModalComponent from "@/features/employees/components/employees-form-modal.component";
import { usePermissionContext, PERMISSIONS } from '@/features/permissions';
import { CONFIG } from '../../../../global-config';

const { Title, Text } = Typography;

/* ─── Detail row helper ─── */
function InfoRow({ icon, label, value }: { icon: React.ReactNode; label: string; value?: React.ReactNode }) {
  return (
    <div style={{ display: 'flex', alignItems: 'flex-start', gap: 12, padding: '12px 0' }}>
      <span style={{ color: '#64748B', fontSize: 16, marginTop: 2 }}>{icon}</span>
      <div style={{ minWidth: 120 }}>
        <Text style={{ color: '#94A3B8', fontSize: 13, display: 'block' }}>{label}</Text>
        <div style={{ marginTop: 2 }}>
          {typeof value === 'string' || !value ? (
            <Text style={{ fontSize: 14, color: value ? '#0F172A' : '#CBD5E1', fontWeight: 500 }}>
              {(value as string) || '—'}
            </Text>
          ) : (
            value
          )}
        </div>
      </div>
    </div>
  );
}

/* ─── Role color mapping ─── */
function getRoleStyle(role: string): { color: string; bg: string } {
  switch (role?.toLowerCase()) {
    case 'admin':
      return { color: '#7C3AED', bg: '#F5F3FF' };
    case 'manager':
      return { color: '#2563EB', bg: '#EFF6FF' };
    case 'employee':
    default:
      return { color: '#64748B', bg: '#F1F5F9' };
  }
}

export default function EmployeesDetailsComponent() {
    const { t } = useTranslate('employees');
    const { t: tCommon } = useTranslate('common');
    const service = container.resolve(EmployeesService);
    const { id: employeeId } = useParams();
    const router = useRouter();
    const [showResetPassword, setShowResetPassword] = useState(false);
    const { activeModal, openModal, closeModal } = useModalStore();
    const { hasPermission, loading: permissionLoading } = usePermissionContext();

    // Permission checks
    const canViewEmployeeDetail = hasPermission(PERMISSIONS.EMPLOYEE_VIEW_DETAIL);
    const canEditEmployee = hasPermission(PERMISSIONS.EMPLOYEE_EDIT);

    const { data: employee } = useQuery({
        queryKey: ['employee-details', employeeId],
        queryFn: () => service.getDetails(employeeId as string),
        enabled: !!employeeId && canViewEmployeeDetail,
    });

    // Access denied for users without view permission
    if (!permissionLoading && !canViewEmployeeDetail) {
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

    const handleEditEmployee = () => {
        openModal('addEmployee');
    };

    /* ─── Initials for Avatar fallback ─── */
    const initials = (employee?.name || '')
      .split(' ')
      .map((n: string) => n[0])
      .join('')
      .slice(0, 2)
      .toUpperCase();

    const roleStyle = getRoleStyle(employee?.role || '');

    return (
        <>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 24 }}>
            {/* ── Profile Card ── */}
            <Card
              styles={{ body: { padding: 0 } }}
              style={{ borderRadius: 16, overflow: 'hidden', border: '1px solid #E2E8F0' }}
            >
              {/* Header banner */}
              <div
                style={{
                  background: 'linear-gradient(135deg, #1B3A5C 0%, #2D5A8E 100%)',
                  padding: '32px 32px 24px',
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'flex-start',
                }}
              >
                <div style={{ display: 'flex', alignItems: 'center', gap: 20 }}>
                  <Avatar
                    src={employee?.userProfileUrl || undefined}
                    size={72}
                    style={{
                      backgroundColor: '#F59E0B',
                      fontSize: 24,
                      fontWeight: 700,
                      border: '3px solid rgba(255,255,255,0.3)',
                    }}
                  >
                    {initials}
                  </Avatar>
                  <div>
                    <Title level={4} style={{ color: '#fff', margin: 0 }}>
                      {employee?.name}
                    </Title>
                    <Tag
                      style={{
                        marginTop: 6,
                        borderRadius: 6,
                        fontWeight: 600,
                        fontSize: 12,
                        border: 'none',
                        background: 'rgba(255,255,255,0.2)',
                        color: '#fff',
                      }}
                    >
                      {employee?.role?.toUpperCase()}
                    </Tag>
                  </div>
                </div>
                {canEditEmployee && (
                  <Space size={8}>
                    <Button
                      icon={<LockOutlined />}
                      onClick={() => setShowResetPassword(true)}
                      style={{
                        background: 'rgba(255,255,255,0.1)',
                        borderColor: 'rgba(255,255,255,0.25)',
                        color: '#fff',
                        borderRadius: 10,
                        height: 40,
                        paddingInline: 16,
                        fontWeight: 500,
                      }}
                    >
                      {t('resetPassword')}
                    </Button>
                    <Button
                      icon={<EditOutlined />}
                      onClick={handleEditEmployee}
                      style={{
                        background: 'rgba(255,255,255,0.15)',
                        borderColor: 'rgba(255,255,255,0.3)',
                        color: '#fff',
                        borderRadius: 10,
                        height: 40,
                        paddingInline: 20,
                        fontWeight: 500,
                      }}
                    >
                      {t('editEmployee')}
                    </Button>
                  </Space>
                )}
              </div>

              {/* Info fields */}
              {employee && (
                <div style={{ padding: '24px 32px 28px' }}>
                  <Row gutter={[48, 0]}>
                    <Col xs={24} md={12}>
                      <InfoRow
                        icon={<SafetyOutlined />}
                        label={t('role')}
                        value={
                          <Tag
                            style={{
                              borderRadius: 6,
                              fontWeight: 600,
                              fontSize: 13,
                              padding: '2px 12px',
                              border: `1px solid ${roleStyle.color}30`,
                              background: roleStyle.bg,
                              color: roleStyle.color,
                            }}
                          >
                            {employee.role}
                          </Tag>
                        }
                      />
                      <InfoRow icon={<PhoneOutlined />} label={t('phone')} value={employee.phone} />
                      <InfoRow icon={<MailOutlined />} label={t('email')} value={employee.email} />
                    </Col>
                    <Col xs={24} md={12}>
                      <InfoRow icon={<EnvironmentOutlined />} label={t('address')} value={employee.address} />
                      <InfoRow icon={<FileTextOutlined />} label={t('description')} value={employee.description} />
                      <InfoRow
                        icon={employee.isActive ? <CheckCircleOutlined /> : <CloseCircleOutlined />}
                        label={t('status')}
                        value={
                          <Tag
                            color={employee.isActive ? 'success' : 'error'}
                            style={{ borderRadius: 6, fontWeight: 600, fontSize: 13, padding: '2px 12px' }}
                          >
                            {employee.isActive ? t('active') : t('deactive')}
                          </Tag>
                        }
                      />
                    </Col>
                  </Row>
                </div>
              )}
            </Card>
          </div>

          {showResetPassword && canEditEmployee && (
                <EmployeesResetPasswordModalComponent
                    open={showResetPassword}
                    employeeId={Number(employeeId)}
                    onClose={() => setShowResetPassword(false)}
                />
            )}
            {activeModal === 'addEmployee' && canEditEmployee && (
                <EmployeesFormModalComponent
                    open={true}
                    onClose={closeModal}
                    initialValues={employee}
                />
            )}
        </>
    );
}
