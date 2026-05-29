'use client';

import 'reflect-metadata';
import { Avatar, Button, Card, Typography, Tag, Row, Col, App, Empty } from 'antd';
import {
  PaperClipOutlined, EditOutlined, UserOutlined,
  DollarOutlined, CreditCardOutlined, FileTextOutlined,
  CalendarOutlined, DeleteOutlined,
} from '@ant-design/icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { container } from 'tsyringe';
import { OrdersService } from '@/features/orders/services/orders.service';
import { useTranslate } from '@/locales/use-locales';
import LoadingSpinnerComponent from '@/components/loading/loading-spinner.component';
import { useState } from 'react';
import PaymentFormModalComponent from './payment-form-modal.component';
import { ConfirmDeleteModalComponent } from '@/components/modal/confirm-delete-modal.component';
import { useParams, useRouter } from 'next/navigation';
import { paths } from '@/routes/paths';
import { FilePreviewCard } from '@/components/file-preview/file-preview-card.component';
import dayjs from 'dayjs';

const { Title, Text } = Typography;

function getPaymentTypeStyle(paymentType: string) {
  const styles: Record<string, { bg: string; text: string; border: string }> = {
    Cash: { bg: '#EFF6FF', text: '#2563EB', border: '#BFDBFE' },
    Transfer: { bg: '#F0FDFA', text: '#0D9488', border: '#99F6E4' },
    Cheque: { bg: '#FFFBEB', text: '#D97706', border: '#FDE68A' },
    Discount: { bg: '#FEF2F2', text: '#DC2626', border: '#FECACA' },
  };
  return styles[paymentType] || { bg: '#F1F5F9', text: '#64748B', border: '#E2E8F0' };
}

/* ─── Info row helper ─── */
function InfoRow({ icon, label, value, valueElement }: { icon: React.ReactNode; label: string; value?: string | null; valueElement?: React.ReactNode }) {
  return (
    <div style={{ display: 'flex', alignItems: 'flex-start', gap: 12, padding: '14px 0' }}>
      <span style={{ color: '#64748B', fontSize: 16, marginTop: 2 }}>{icon}</span>
      <div style={{ minWidth: 120 }}>
        <Text style={{ color: '#94A3B8', fontSize: 13, display: 'block' }}>{label}</Text>
        {valueElement || (
          <Text style={{ fontSize: 14, color: value ? '#0F172A' : '#CBD5E1', fontWeight: 500 }}>
            {value || '—'}
          </Text>
        )}
      </div>
    </div>
  );
}

interface Props {
  type: 'client' | 'provider';
}

export default function PaymentDetailPageComponent({ type }: Props) {
  const { t } = useTranslate('orders');
  const { notification } = App.useApp();
  const queryClient = useQueryClient();
  const service = container.resolve(OrdersService);
  const router = useRouter();
  const params = useParams();

  const orderId = params.orderId as string;
  const paymentId = params.paymentId as string;

  const [editModalOpen, setEditModalOpen] = useState(false);
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);

  const queryKey = type === 'client' ? 'client-payment-detail' : 'provider-payment-detail';

  const { data: paymentDetail, isLoading } = useQuery({
    queryKey: [queryKey, orderId, paymentId],
    queryFn: () => (type === 'client' ? service.getClientPaymentDetail(orderId, paymentId) : service.getProviderPaymentDetail(orderId, paymentId)),
    enabled: !!paymentId && !!orderId,
  });

  const { data: orderDetails } = useQuery({
    queryKey: ['order-edit-details', orderId],
    queryFn: () => service.getDetails(orderId),
    enabled: !!orderId,
  });

  const currency = type === 'client' ? orderDetails?.sellCurrency : orderDetails?.buyCurrency;

  const deleteMutation = useMutation({
    mutationFn: async () => {
      if (type === 'client') {
        await service.deleteClientPayment(orderId, paymentId);
      } else {
        await service.deleteProviderPayment(orderId, paymentId);
      }
    },
    onSuccess: async () => {
      notification.success({ message: t('paymentDeletedSuccessfully') });
      await queryClient.invalidateQueries({
        queryKey: [type === 'client' ? 'client-payments' : 'provider-payments', orderId],
      });
      setDeleteModalOpen(false);
      router.push(paths.panel.orderDetails(orderId));
    },
  });

  if (isLoading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: 300 }}>
        <LoadingSpinnerComponent />
      </div>
    );
  }

  const ptStyle = getPaymentTypeStyle(paymentDetail?.paymentType || '');
  const formattedAmount = paymentDetail?.amount?.toLocaleString('en-US', { minimumFractionDigits: 2 }) || '0.00';

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 24 }}>
      {/* ── Payment Info Card ── */}
      <Card
        styles={{ body: { padding: 0 } }}
        style={{ borderRadius: 16, overflow: 'hidden', border: '1px solid #E2E8F0' }}
      >
        {/* Header: user + edit button */}
        <div style={{
          padding: '24px 32px 20px',
          borderBottom: '1px solid #F1F5F9',
          display: 'flex', justifyContent: 'space-between', alignItems: 'center',
        }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 16 }}>
            <Avatar
              size={52}
              src={paymentDetail?.userProfileUrl || undefined}
              icon={<UserOutlined />}
              style={{ backgroundColor: '#3B82F6', fontSize: 20 }}
            />
            <div>
              <Title level={4} style={{ margin: 0 }}>
                {paymentDetail?.userFullName || '—'}
              </Title>
              <Text style={{ color: '#94A3B8', fontSize: 13 }}>
                {type === 'client' ? t('clientPaymentDetail') : t('providerPaymentDetail')}
              </Text>
            </div>
          </div>
          <div style={{ display: 'flex', gap: 10 }}>
            <Button
              icon={<EditOutlined />}
              onClick={() => setEditModalOpen(true)}
              style={{
                borderRadius: 10, height: 40, paddingInline: 20, fontWeight: 500,
                borderColor: '#E2E8F0', color: '#475569',
              }}
            >
              {t('editPayment')}
            </Button>
            <Button
              icon={<DeleteOutlined />}
              onClick={() => setDeleteModalOpen(true)}
              style={{
                borderRadius: 10, height: 40, paddingInline: 20, fontWeight: 500,
                borderColor: '#FECACA', color: '#DC2626', background: '#FEF2F2',
              }}
            >
              {t('deletePayment')}
            </Button>
          </div>
        </div>

        {/* Payment details grid */}
        <div style={{ padding: '20px 32px 28px' }}>
          {/* Amount highlight card */}
          <div style={{
            background: '#F8FAFC', borderRadius: 12, padding: '20px 24px',
            border: '1px solid #F1F5F9', marginBottom: 24,
            display: 'flex', justifyContent: 'space-between', alignItems: 'center',
          }}>
            <div>
              <Text style={{ color: '#94A3B8', fontSize: 12, display: 'block', marginBottom: 4 }}>{t('amount')}</Text>
              <Text style={{ fontSize: 28, fontWeight: 700, color: '#0F172A' }}>
                {formattedAmount}
              </Text>
              {currency && (
                <Text style={{ color: '#94A3B8', fontSize: 14, marginLeft: 8 }}>{currency}</Text>
              )}
            </div>
            <Tag
              style={{
                borderRadius: 8, padding: '4px 16px', fontWeight: 600, fontSize: 13,
                border: `1px solid ${ptStyle.border}`,
                background: ptStyle.bg, color: ptStyle.text, margin: 0,
              }}
            >
              {paymentDetail?.paymentType || '—'}
            </Tag>
          </div>

          {/* Info rows */}
          <Row gutter={[48, 0]}>
            <Col xs={24} md={12}>
              <InfoRow
                icon={<CreditCardOutlined />}
                label={t('paymentType')}
                valueElement={
                  <Tag
                    style={{
                      borderRadius: 6, padding: '2px 12px', marginTop: 4,
                      background: ptStyle.bg, color: ptStyle.text,
                      border: `1px solid ${ptStyle.border}`,
                    }}
                  >
                    {paymentDetail?.paymentType || '—'}
                  </Tag>
                }
              />
              <InfoRow
                icon={<CalendarOutlined />}
                label={t('date')}
                value={paymentDetail?.createdAt ? dayjs(paymentDetail.createdAt).format('MMM DD, YYYY • h:mm A') : undefined}
              />
            </Col>
            <Col xs={24} md={12}>
              <InfoRow
                icon={<FileTextOutlined />}
                label={t('description')}
                value={paymentDetail?.description}
              />
            </Col>
          </Row>
        </div>
      </Card>

      {/* ── Attachments Card ── */}
      <Card
        style={{ borderRadius: 16, border: '1px solid #E2E8F0' }}
        styles={{ body: { padding: '24px 32px' } }}
      >
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
          <Title level={5} style={{ margin: 0 }}>{t('attachments')}</Title>
        </div>

        {paymentDetail?.files?.length ? (
          <div style={{ display: 'flex', flexWrap: 'wrap', gap: 12 }}>
            {paymentDetail.files.map((att: any) => (
              <FilePreviewCard key={att.id} file={{ id: att.id, fileName: att.name, url: att.url }} />
            ))}
          </div>
        ) : (
          <div style={{ padding: 40, borderRadius: 12, border: '1px solid #E2E8F0', background: '#FAFBFC', textAlign: 'center' }}>
            <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description={<Text style={{ color: '#94A3B8' }}>{t('noAttachments')}</Text>} />
          </div>
        )}
      </Card>

      <PaymentFormModalComponent open={editModalOpen} onClose={() => setEditModalOpen(false)} orderId={orderId} paymentId={Number(paymentId)} type={type} currency={currency} />

      <ConfirmDeleteModalComponent
        open={deleteModalOpen}
        onCancel={() => setDeleteModalOpen(false)}
        onConfirm={() => deleteMutation.mutateAsync()}
        title={t('deletePayment')}
        loading={deleteMutation.isPending}
        description={
          <>
            {t('areYouSureYouWantToDeletePayment')} <strong>{paymentDetail?.userFullName}</strong>?
          </>
        }
      />
    </div>
  );
}
