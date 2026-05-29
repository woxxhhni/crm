'use client';

import { Avatar, Button, Card, Typography, App, Space, Empty, Tag, Row, Col, Tooltip } from 'antd';
import {
  DeleteOutlined, EditOutlined,
  UserOutlined, DollarOutlined, WalletOutlined,
  BankOutlined, PlusOutlined,
} from '@ant-design/icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { container } from 'tsyringe';
import { OrdersService } from '@/features/orders/services/orders.service';
import { useTranslate } from '@/locales/use-locales';
import LoadingSpinnerComponent from '@/components/loading/loading-spinner.component';
import { useState } from 'react';
import { ConfirmDeleteModalComponent } from '@/components/modal/confirm-delete-modal.component';
import type { PaymentTransaction } from '@/api/orders/types';
import { useRouter } from 'next/navigation';
import { paths } from '@/routes/paths';
import { formatDateForDisplay } from '@/utils/date-utils';
import ExtraProviderPaymentFormModal from './extra-provider-payment-form-modal.component';
import dayjs from 'dayjs';

const { Title, Text } = Typography;

function getPaymentTypeColor(paymentType: string) {
    const colors: Record<string, { bg: string; text: string; border: string }> = {
        Cash: { bg: '#EFF6FF', text: '#2563EB', border: '#BFDBFE' },
        Transfer: { bg: '#F0FDFA', text: '#0D9488', border: '#99F6E4' },
        Cheque: { bg: '#FFFBEB', text: '#D97706', border: '#FDE68A' },
        Discount: { bg: '#FEF2F2', text: '#DC2626', border: '#FECACA' },
    };
    return colors[paymentType] || { bg: '#F1F5F9', text: '#64748B', border: '#E2E8F0' };
}

interface Props {
    orderId: string | number;
    epId: string | number;
}

export default function ExtraProviderDetailsComponent({ orderId, epId }: Props) {
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

    const { data: summaryData, isLoading } = useQuery({
        queryKey: ['extra-provider-payments', orderId, epId],
        queryFn: () => service.getExtraProviderPaymentSummary(orderId, epId),
        enabled: !!orderId && !!epId,
    });

    const deleteMutation = useMutation({
        mutationFn: async () => {
            if (!paymentToDelete) return;
            await service.deleteExtraProviderPayment(orderId, paymentToDelete.id);
        },
        onSuccess: async () => {
            notification.success({ message: t('paymentDeletedSuccessfully') });
            await queryClient.invalidateQueries({ queryKey: ['extra-provider-payments', orderId, epId] });
            setDeleteModalOpen(false);
            setPaymentToDelete(null);
        },
    });

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
            <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: 300 }}>
                <LoadingSpinnerComponent />
            </div>
        );
    }

    const totalBalance = summaryData?.totalBalance || 0;
    const paidAmount = summaryData?.paidAmount || 0;
    const currentBalance = summaryData?.currentBalance || 0;
    const currency = summaryData?.currency || '';

    return (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 24 }}>
            {/* ── Provider Info Card ── */}
            <Card
                styles={{ body: { padding: 0 } }}
                style={{ borderRadius: 16, overflow: 'hidden', border: '1px solid #E2E8F0' }}
            >
                {/* Provider identity header */}
                <div style={{
                    padding: '24px 32px 20px',
                    borderBottom: '1px solid #F1F5F9',
                    display: 'flex', alignItems: 'center', gap: 16,
                }}>
                    <Avatar
                        size={52}
                        icon={<UserOutlined />}
                        style={{ backgroundColor: '#10B981', fontSize: 22 }}
                    />
                    <div>
                        <Title level={4} style={{ margin: 0 }}>
                            {summaryData?.name || t('extraProviderDetails')}
                        </Title>
                        <Text style={{ color: '#94A3B8', fontSize: 13 }}>{t('extraProviderDetails')}</Text>
                    </div>
                </div>

                {/* Balance summary cards */}
                <div style={{ padding: '20px 32px 28px' }}>
                    <Row gutter={16}>
                        <Col xs={24} sm={8}>
                            <div style={{
                                background: '#F8FAFC', borderRadius: 12, padding: '18px 20px',
                                border: '1px solid #F1F5F9',
                            }}>
                                <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 8 }}>
                                    <DollarOutlined style={{ color: '#64748B', fontSize: 14 }} />
                                    <Text style={{ color: '#94A3B8', fontSize: 12 }}>{t('totalBalance')}</Text>
                                </div>
                                <Text style={{ fontSize: 20, fontWeight: 700, color: '#0F172A' }}>
                                    {totalBalance.toLocaleString('en-US', { minimumFractionDigits: 2 })}
                                </Text>
                                <Text style={{ color: '#94A3B8', fontSize: 13, marginLeft: 6 }}>{currency}</Text>
                            </div>
                        </Col>
                        <Col xs={24} sm={8}>
                            <div style={{
                                background: '#F0FDF4', borderRadius: 12, padding: '18px 20px',
                                border: '1px solid #BBF7D0',
                            }}>
                                <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 8 }}>
                                    <WalletOutlined style={{ color: '#16A34A', fontSize: 14 }} />
                                    <Text style={{ color: '#16A34A', fontSize: 12 }}>{t('paidAmount')}</Text>
                                </div>
                                <Text style={{ fontSize: 20, fontWeight: 700, color: '#059669' }}>
                                    {paidAmount.toLocaleString('en-US', { minimumFractionDigits: 2 })}
                                </Text>
                                <Text style={{ color: '#059669', fontSize: 13, marginLeft: 6 }}>{currency}</Text>
                            </div>
                        </Col>
                        <Col xs={24} sm={8}>
                            <div style={{
                                background: '#FEF2F2', borderRadius: 12, padding: '18px 20px',
                                border: '1px solid #FECACA',
                            }}>
                                <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 8 }}>
                                    <BankOutlined style={{ color: '#DC2626', fontSize: 14 }} />
                                    <Text style={{ color: '#DC2626', fontSize: 12 }}>{t('currentBalance')}</Text>
                                </div>
                                <Text style={{ fontSize: 20, fontWeight: 700, color: '#DC2626' }}>
                                    {currentBalance.toLocaleString('en-US', { minimumFractionDigits: 2 })}
                                </Text>
                                <Text style={{ color: '#DC2626', fontSize: 13, marginLeft: 6 }}>{currency}</Text>
                            </div>
                        </Col>
                    </Row>
                </div>
            </Card>

            {/* ── Transactions Card ── */}
            <Card
                style={{ borderRadius: 16, border: '1px solid #E2E8F0' }}
                styles={{ body: { padding: '24px' } }}
            >
                {/* Section Header */}
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
                    <Title level={5} style={{ margin: 0 }}>{t('transactions')}</Title>
                    <Button
                        type='primary'
                        icon={<PlusOutlined />}
                        onClick={() => setAddModalOpen(true)}
                        style={{ borderRadius: 10, height: 40, paddingInline: 20 }}
                    >
                        {t('addPayment')}
                    </Button>
                </div>

                {/* Transactions List */}
                {summaryData?.transactions?.length ? (
                    <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
                        {summaryData.transactions.map((transaction) => {
                            const ptColor = getPaymentTypeColor(transaction.paymentType);
                            return (
                                <div
                                    key={transaction.id}
                                    style={{
                                        display: 'flex', justifyContent: 'space-between', alignItems: 'center',
                                        padding: '14px 20px', border: '1px solid #F1F5F9', borderRadius: 12,
                                        background: '#FAFBFC', transition: 'all 0.15s',
                                    }}
                                    className='hover:border-blue-200'
                                >
                                    {/* Left: user + type */}
                                    <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
                                        <Avatar
                                            size={40}
                                            src={transaction.userProfileUrl || undefined}
                                            icon={<UserOutlined />}
                                            style={{ backgroundColor: '#3B82F6' }}
                                        />
                                        <div>
                                            <Text style={{ fontWeight: 500, display: 'block', fontSize: 14 }}>{transaction.userFullName}</Text>
                                            <Tag
                                                style={{
                                                    borderRadius: 6, fontSize: 11, padding: '0 8px', marginTop: 4,
                                                    background: ptColor.bg, color: ptColor.text,
                                                    border: `1px solid ${ptColor.border}`,
                                                }}
                                            >
                                                {transaction.paymentType}
                                            </Tag>
                                        </div>
                                    </div>

                                    {/* Center: date */}
                                    <div style={{ textAlign: 'center' }}>
                                        <Text style={{ color: '#94A3B8', fontSize: 13 }}>
                                            {dayjs(transaction.createdAt).format('MMM DD, YYYY')}
                                        </Text>
                                    </div>

                                    {/* Right: amount + actions */}
                                    <div style={{ display: 'flex', alignItems: 'center', gap: 20 }}>
                                        <div style={{ textAlign: 'right' }}>
                                            <Text style={{ fontWeight: 600, fontSize: 16, display: 'block', color: '#0F172A' }}>
                                                {transaction.amount?.toLocaleString('en-US', { minimumFractionDigits: 2 })}
                                            </Text>
                                            <Text style={{ color: '#94A3B8', fontSize: 12 }}>
                                                {t('remaining')}: {transaction.remainingAmount?.toLocaleString('en-US', { minimumFractionDigits: 2 })}
                                            </Text>
                                        </div>
                                        <Space size='small'>
                                            <Tooltip title={t('editPayment')}>
                                                <Button
                                                    type='text'
                                                    shape='circle'
                                                    aria-label={t('editPayment')}
                                                    onClick={() => handleEditPayment(transaction.id)}
                                                    icon={<EditOutlined />}
                                                    style={{ color: '#64748B' }}
                                                />
                                            </Tooltip>
                                            <Tooltip title={t('deletePayment')}>
                                                <Button
                                                    type='text'
                                                    shape='circle'
                                                    aria-label={t('deletePayment')}
                                                    icon={<DeleteOutlined />}
                                                    style={{ color: '#EF4444' }}
                                                    onClick={() => handleDeleteClick(transaction)}
                                                />
                                            </Tooltip>
                                        </Space>
                                    </div>
                                </div>
                            );
                        })}
                    </div>
                ) : (
                    <div style={{ padding: 48, borderRadius: 14, border: '1px solid #E2E8F0', background: '#FAFBFC', textAlign: 'center' }}>
                        <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description={<Text style={{ color: '#94A3B8' }}>{t('noTransactionsYet')}</Text>} />
                    </div>
                )}
            </Card>

            <ExtraProviderPaymentFormModal
                open={addModalOpen}
                onClose={() => setAddModalOpen(false)}
                orderId={orderId}
                epId={epId}
                currency={summaryData?.currency}
            />

            <ExtraProviderPaymentFormModal
                open={editModalOpen}
                onClose={() => {
                    setEditModalOpen(false);
                    setSelectedPaymentId(null);
                }}
                orderId={orderId}
                epId={epId}
                paymentId={selectedPaymentId}
                currency={summaryData?.currency}
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
