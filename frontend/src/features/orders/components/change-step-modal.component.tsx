'use client';

import { Form, Input, DatePicker, Select, App } from 'antd';
import { useState, useEffect, useCallback, useMemo } from 'react';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm, Controller, SubmitHandler } from 'react-hook-form';
import AppModal from '@/components/modal/app-modal.component';
import AppUploadDraggerComponent from '@/components/form/upload/app-upload-dragger.component';
import { container } from 'tsyringe';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useTranslate } from '@/locales/use-locales';
import { OrdersService } from '@/features/orders/services/orders.service';
import dayjs from 'dayjs';
import { defaultSelectFilter } from '@/utils/select-utils';
import { formatDateForAPI } from '@/utils/date-utils';
import { translateWorkflowLabel } from '../utils/workflow-labels';

const createSchema = (t: (key: string, params?: Record<string, string>) => string) => z.object({
    stepId: z.number({ message: t('fieldIsRequired', { field: t('targetStep') }) }).min(1, t('fieldIsRequired', { field: t('targetStep') })),
    actionDate: z.any().refine((val) => val, { message: t('fieldIsRequired', { field: t('date') }) }),
    description: z.string().optional(),
});

export type ChangeStepForm = z.infer<ReturnType<typeof createSchema>>;

interface Props {
    open: boolean;
    onClose: () => void;
    orderId: string | number;
}

export default function ChangeStepModalComponent({ open, onClose, orderId }: Props) {
    const [fileList, setFileList] = useState<any[]>([]);
    const { t } = useTranslate('orders');
    const { notification } = App.useApp();
    const queryClient = useQueryClient();
    const service = container.resolve(OrdersService);

    const schema = createSchema(t);

    const { control, reset, handleSubmit, formState: { errors } } = useForm<ChangeStepForm>({
        resolver: zodResolver(schema),
        defaultValues: {
            stepId: undefined,
            actionDate: dayjs(),
            description: '',
        },
    });

    // Fetch steps
    const { data: stepOptions, isLoading: stepsLoading } = useQuery({
        queryKey: ['steps-list'],
        queryFn: () => service.getStepOptions(),
        enabled: open,
    });

    const translatedStepOptions = useMemo(
        () => stepOptions?.map((option: any) => ({
            ...option,
            label: translateWorkflowLabel(String(option.label ?? ''), t),
        })),
        [stepOptions, t],
    );

    useEffect(() => {
        if (!open) {
            reset({
                stepId: undefined,
                actionDate: dayjs(),
                description: '',
            });
            setFileList([]);
        }
    }, [open, reset]);

    const createMutation = useMutation({
        mutationFn: async (data: ChangeStepForm) => {
            const files = fileList
                .filter(f => f.originFileObj)
                .map(f => f.originFileObj as File);

            await service.setStep({
                orderId,
                stepId: data.stepId,
                actionDate: formatDateForAPI(data.actionDate),
                description: data.description || '',
                files: files.length > 0 ? files : undefined,
            });
        },
        onSuccess: async () => {
            notification.success({ message: t('stepChangedSuccessfully') });
            await queryClient.invalidateQueries({ queryKey: ['order-edit-details', String(orderId)] });
            onClose();
        },
    });

    const handleSubmitForm: SubmitHandler<ChangeStepForm> = async (data) => {
        await createMutation.mutateAsync(data);
    };

    const handleUploadChange = useCallback(
        ({ fileList: newFileList }: any) => {
            setFileList(newFileList);
        },
        []
    );

    return (
        <AppModal
            open={open}
            onClose={onClose}
            title={t('changeStep')}
            onSubmit={handleSubmit(handleSubmitForm)}
            submitText={t('change')}
            width={648}
            loading={createMutation.isPending}
        >
            <Form layout="vertical">
                <Form.Item
                    label={t('targetStep')}
                    required
                    validateStatus={errors.stepId ? 'error' : ''}
                    help={errors.stepId?.message}
                >
                    <Controller
                        name="stepId"
                        control={control}
                        render={({ field }) => (
                            <Select
                                {...field}
                                showSearch
                                filterOption={defaultSelectFilter}
                                placeholder={t('selectTargetStep')}
                                options={translatedStepOptions}
                                loading={stepsLoading}
                                status={errors.stepId ? 'error' : ''}
                            />
                        )}
                    />
                </Form.Item>

                <Form.Item
                    label={t('actionDate')}
                    required
                    validateStatus={errors.actionDate ? 'error' : ''}
                    help={errors.actionDate?.message as string}
                >
                    <Controller
                        name="actionDate"
                        control={control}
                        render={({ field }) => (
                            <DatePicker
                                {...field}
                                value={field.value ? dayjs(field.value) : null}
                                onChange={(date) => field.onChange(date)}
                                style={{ width: '100%' }}
                                placeholder={t('selectDate')}
                                status={errors.actionDate ? 'error' : ''}
                            />
                        )}
                    />
                </Form.Item>

                <Form.Item
                    label={t('descriptionOptional')}
                    validateStatus={errors.description ? 'error' : ''}
                    help={errors.description?.message}
                >
                    <Controller
                        name="description"
                        control={control}
                        render={({ field }) => (
                            <Input.TextArea
                                {...field}
                                placeholder={t('writeDescription')}
                                maxLength={500}
                                showCount
                                rows={4}
                            />
                        )}
                    />
                </Form.Item>

                <Form.Item label={t('attachmentsOptional')}>
                    <AppUploadDraggerComponent
                        fileList={fileList}
                        onChange={handleUploadChange}
                    />
                </Form.Item>
            </Form>
        </AppModal>
    );
}
