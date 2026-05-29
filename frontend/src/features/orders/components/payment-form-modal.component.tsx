'use client';

import { Form, Input, Select, App } from 'antd';
import { useState, useEffect, useCallback } from 'react';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm, Controller, SubmitHandler } from 'react-hook-form';
import AppModal from '@/components/modal/app-modal.component';
import AppUploadDraggerComponent from '@/components/form/upload/app-upload-dragger.component';
import { AppInput } from '@/components/form/amount-input';
import { container } from 'tsyringe';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useTranslate } from '@/locales/use-locales';
import { OrdersService } from '@/features/orders/services/orders.service';
import type { PaymentCreateDTO, PaymentUpdateDTO } from '@/api/orders/types';
import { defaultSelectFilter } from '@/utils/select-utils';

const createSchema = (t: (key: string, params?: Record<string, string>) => string) => z.object({
    amount: z.union([z.string(), z.number()]).refine((val) => val && Number(val) > 0, { message: t('fieldIsRequired', { field: t('amount') }) }),
    paymentType: z.string().min(1, t('fieldIsRequired', { field: t('paymentType') })),
    description: z.string().optional(),
});

export type PaymentForm = z.infer<ReturnType<typeof createSchema>>;

interface Props {
    open: boolean;
    onClose: () => void;
    orderId: string | number;
    paymentId?: number | null;
    type: 'client' | 'provider';
    currency?: string;
}

const PAYMENT_TYPES = [
    { label: 'Cheque', value: '1' },
    { label: 'Transfer', value: '2' },
    { label: 'Cash', value: '3' },
    { label: 'Discount', value: '4' },
];

export default function PaymentFormModalComponent({ open, onClose, orderId, paymentId, type, currency }: Props) {
    const [fileList, setFileList] = useState<any[]>([]);
    const [removedFileIds, setRemovedFileIds] = useState<number[]>([]);
    const { t } = useTranslate('orders');
    const { notification } = App.useApp();
    const queryClient = useQueryClient();
    const service = container.resolve(OrdersService);

    const isEditMode = !!paymentId;
    const schema = createSchema(t);

    const { control, reset, handleSubmit, formState: { errors } } = useForm<PaymentForm>({
        resolver: zodResolver(schema),
        defaultValues: {
            amount: '',
            paymentType: '',
            description: '',
        },
    });

    const { data: paymentDetail, isLoading: isDetailLoading } = useQuery({
        queryKey: [type === 'client' ? 'client-payment-detail' : 'provider-payment-detail', orderId, paymentId],
        queryFn: () => type === 'client'
            ? service.getClientPaymentDetail(orderId, paymentId!)
            : service.getProviderPaymentDetail(orderId, paymentId!),
        enabled: !!paymentId && open,
    });

    useEffect(() => {
        if (paymentDetail && isEditMode && open) {
            reset({
                amount: paymentDetail.amount?.toString() ?? '',
                paymentType: paymentDetail.paymentType ?? '',
                description: paymentDetail.description ?? '',
            });
            // Set existing attachments
            if (paymentDetail.files?.length) {
                const existingFilesData = paymentDetail.files.map((att: any) => ({
                    uid: String(att.id),
                    name: att.name || att.fileName || 'File',
                    status: 'done' as const,
                    url: att.url,
                    id: att.id,
                }));
                setFileList(existingFilesData);
            } else {
                setFileList([]);
            }
            setRemovedFileIds([]);
        }
    }, [paymentDetail, isEditMode, open, reset]);

    useEffect(() => {
        if (!open) {
            reset({
                amount: '',
                paymentType: '',
                description: '',
            });
            setFileList([]);
            setRemovedFileIds([]);
        }
    }, [open, reset]);

    const createMutation = useMutation({
        mutationFn: async (data: PaymentForm) => {
            const newFiles = fileList
                .filter(f => f.originFileObj)
                .map(f => f.originFileObj as File);
            const dto: PaymentCreateDTO = {
                amount: Number(data.amount),
                paymentType: data.paymentType,
                description: data.description,
                files: newFiles,
            };
            if (type === 'client') {
                await service.createClientPayment(orderId, dto);
            } else {
                await service.createProviderPayment(orderId, dto);
            }
        },
        onSuccess: async () => {
            notification.success({ message: t('paymentCreatedSuccessfully') });
            await queryClient.invalidateQueries({
                queryKey: [type === 'client' ? 'client-payments' : 'provider-payments', orderId]
            });
            onClose();
        },
    });

    const updateMutation = useMutation({
        mutationFn: async (data: PaymentForm) => {
            // Combine existing file IDs and new files
            const filesArray: (File | number)[] = [];

            // Add existing file IDs that haven't been removed
            fileList.forEach(f => {
                if (f.url && !f.originFileObj && (f as any).id) {
                    // Add existing file ID
                    filesArray.push((f as any).id);
                } else if (f.originFileObj) {
                    // Add new file
                    filesArray.push(f.originFileObj as File);
                }
            });

            const dto: PaymentUpdateDTO = {
                amount: Number(data.amount),
                paymentType: data.paymentType,
                description: data.description,
                files: filesArray,
                removedFileIds,
            };

            if (type === 'client') {
                await service.updateClientPayment(orderId, paymentId!, dto);
            } else {
                await service.updateProviderPayment(orderId, paymentId!, dto);
            }
        },
        onSuccess: async () => {
            notification.success({ message: t('paymentUpdatedSuccessfully') });
            await queryClient.invalidateQueries({
                queryKey: [type === 'client' ? 'client-payments' : 'provider-payments', String(orderId)]
            });
            await queryClient.invalidateQueries({
                queryKey: [type === 'client' ? 'client-payment-detail' : 'provider-payment-detail', String(orderId), String(paymentId)]
            });
            await queryClient.invalidateQueries({
                queryKey: ['order-edit-details', String(orderId)]
            });
            onClose();
        },
    });

    const handleSubmitForm: SubmitHandler<PaymentForm> = async (data) => {
        if (isEditMode) {
            await updateMutation.mutateAsync(data);
        } else {
            await createMutation.mutateAsync(data);
        }
    };

    const handleUploadChange = useCallback(
        ({ fileList: newFileList }: any) => {
            setFileList(newFileList);
        },
        []
    );


    const handleDeleteExistingFile = useCallback(
        async (file: any) => {
            try {
                // If it's an existing file, add to removedFileIds
                if (file.uid && (file as any).id) {
                    setRemovedFileIds(prev => [...prev, (file as any).id]);
                }
                // Remove from fileList
                setFileList(prev => prev.filter(f => f.uid !== file.uid));
                notification.success({ message: t('fileDeletedSuccessfully') });
            } catch (error) {
                console.error(error);
            }
        },
        [notification, t]
    );

    const modalTitle = isEditMode
        ? t('editPayment')
        : type === 'client'
            ? t('addClientPayment')
            : t('addProviderPayment');

    return (
        <AppModal
            open={open}
            onClose={onClose}
            title={modalTitle}
            onSubmit={handleSubmit(handleSubmitForm)}
            submitText={isEditMode ? t('save') : t('add')}
            width={648}
            loading={createMutation.isPending || updateMutation.isPending || (isEditMode && isDetailLoading)}
        >
            <Form layout="vertical">
                <Form.Item
                    label={currency ? `${t('amount')} (${currency})` : t('amount')}
                    required
                    validateStatus={errors.amount ? 'error' : ''}
                    help={errors.amount?.message}
                >
                    <Controller
                        name="amount"
                        control={control}
                        render={({ field }) => (
                            <AppInput
                                {...field}
                                number
                                separate
                                placeholder={t('writeYourAmount')}
                                status={errors.amount ? 'error' : ''}
                            />
                        )}
                    />
                </Form.Item>

                <Form.Item
                    label={t('paymentType')}
                    required
                    validateStatus={errors.paymentType ? 'error' : ''}
                    help={errors.paymentType?.message}
                >
                    <Controller
                        name="paymentType"
                        control={control}
                        render={({ field }) => (
                            <Select
                                {...field}
                                showSearch
                                filterOption={defaultSelectFilter}
                                options={PAYMENT_TYPES}
                                placeholder={t('selectPaymentType')}
                                status={errors.paymentType ? 'error' : ''}
                            />
                        )}
                    />
                </Form.Item>

                <Form.Item label={t('descriptionOptional')}>
                    <Controller
                        name="description"
                        control={control}
                        render={({ field }) => (
                            <Input.TextArea
                                {...field}
                                placeholder={t('writeDescription')}
                                maxLength={100}
                                showCount
                                rows={3}
                            />
                        )}
                    />
                </Form.Item>

                <Form.Item label={t('attachmentsOptional')}>
                    <AppUploadDraggerComponent
                        fileList={fileList}
                        onChange={handleUploadChange}
                        showDeleteConfirm={isEditMode}
                        onDeleteExistingFile={handleDeleteExistingFile}
                    />
                </Form.Item>
            </Form>
        </AppModal>
    );
}
