'use client';

import { useEffect, useMemo } from 'react';
import { Row, Col, DatePicker, Select, Form } from 'antd';
import { AppInput } from '@/components/form/amount-input';
import { FormProvider, useForm, Controller } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useTranslate } from '@/locales/use-locales';
import AppModalComponent from '@/components/modal/app-modal.component';
import { useQuery } from '@tanstack/react-query';
import { container } from 'tsyringe';
import { OrdersService } from '@/features/orders/services/orders.service';
import { AppTextField } from '@/components/form/app-text-field/app-text-field.component';
import { Icon } from '@/components/iconify/iconify.component';
import { defaultSelectFilter } from '@/utils/select-utils';
import { formatDateForAPI } from '@/utils/date-utils';

const filterSchema = z.object({
    clientIds: z.array(z.union([z.string(), z.number()])).optional(),
    providerIds: z.array(z.union([z.string(), z.number()])).optional(),
    statuses: z.array(z.union([z.string(), z.number()])).optional(),
    stepIds: z.array(z.union([z.string(), z.number()])).optional(),
    fromAmount: z.union([z.string(), z.number()]).optional(),
    toAmount: z.union([z.string(), z.number()]).optional(),
    fromDate: z.string().optional(),
    toDate: z.string().optional(),
    title: z.string().optional(),
});

export type OrdersFilterForm = z.infer<typeof filterSchema>;

interface Props {
    open: boolean;
    onClose: () => void;
    onApply: (filters: OrdersFilterForm) => void;
    currentFilters?: OrdersFilterForm;
}

export default function OrdersFiltersModalComponent({
                                                        open,
                                                        onClose,
                                                        onApply,
                                                        currentFilters,
                                                    }: Props) {
    const { t } = useTranslate('orders');
    const service = container.resolve(OrdersService);

    const methods = useForm<OrdersFilterForm>({
        resolver: zodResolver(filterSchema),
        defaultValues: {
            clientIds: [],
            providerIds: [],
            statuses: [],
            stepIds: [],
            fromAmount: '',
            toAmount: '',
            fromDate: '',
            toDate: '',
            title: '',
        },
    });

    const { data: providersData } = useQuery({
        queryKey: ['providers-option-list'],
        queryFn: () => service.providerOptions(),
    });

    const { data: clientsData } = useQuery({
        queryKey: ['clients-option-list'],
        queryFn: () => service.clientOptions(),
    });
    const { data: statusesData } = useQuery({
        queryKey: ['statuses-option-list'],
        queryFn: () => service.statuseOptions(),
    });
    const statusOptions = useMemo(() => {
        const statusLabelMap: Record<string, string> = {
            completed: t('completed'),
            inprogress: t('inProgress'),
            inProgress: t('inProgress'),
            suspended: t('suspended'),
            canceled: t('canceled'),
        };

        return statusesData?.map((option: any) => {
            const key = String(option.label ?? option.value ?? '').replace(/\s/g, '').toLowerCase();
            return {
                ...option,
                label: statusLabelMap[key] || option.label,
            };
        });
    }, [statusesData, t]);
    const { control, handleSubmit, reset, setValue } = methods;

    const handleReset = () => {
        reset({});
        onApply({});
        onClose();
    };

    const handleApply = (values: OrdersFilterForm) => {
        onApply(values);
        onClose();
    };

    useEffect(() => {
        if (open && currentFilters) {
            reset(currentFilters);
        }
    }, [open, currentFilters, reset]);

    return (
        <AppModalComponent
            open={open}
            onClose={onClose}
            title={t('filters') || 'Filters'}
            width={520}
            submitText={t('apply')}
            cancelText={t('reset')}
            onReset={handleReset}
            onSubmit={handleSubmit(handleApply)}
        >
            <FormProvider {...methods}>
                <form onSubmit={handleSubmit(handleApply)} className="space-y-6">
                    <Form layout="vertical">
                        <Row gutter={16}>
                            <Col span={12}>
                                <Form.Item label={t('dateRange') || 'Date Range'} style={{ marginBottom: 0 }}>
                                    <Controller
                                        name="fromDate"
                                        control={control}
                                        render={() => (
                                            <DatePicker.RangePicker
                                                style={{ width: '100%' }}
                                                format="YYYY-MM-DD"
                                                placeholder={[t('startDate'), t('endDate')]}
                                                onChange={(dates) => {
                                                    setValue('fromDate', dates?.[0] ? formatDateForAPI(dates[0]) : '');
                                                    setValue('toDate', dates?.[1] ? formatDateForAPI(dates[1]) : '');
                                                }}
                                            />
                                        )}
                                    />
                                </Form.Item>
                            </Col>

                            <Col span={12}>
                                <Form.Item label={t('totalAmount') || 'Total Amount'} style={{ marginBottom: 0 }}>
                                    <Row gutter={8} align="middle">
                                        <Col span={10}>
                                            <Controller
                                                name="fromAmount"
                                                control={control}
                                                render={({ field }) => (
                                                    <AppInput {...field} number separate placeholder={t('from')} style={{ width: '100%' }} />
                                                )}
                                            />
                                        </Col>
                                        <Col span={4} style={{ textAlign: 'center' }}>
                                            <Icon icon="toArrow" width={12} height={12} />
                                        </Col>
                                        <Col span={10}>
                                            <Controller
                                                name="toAmount"
                                                control={control}
                                                render={({ field }) => (
                                                    <AppInput {...field} number separate placeholder={t('to')} style={{ width: '100%' }} />
                                                )}
                                            />
                                        </Col>
                                    </Row>
                                </Form.Item>
                            </Col>
                        </Row>

                        <div className="my-4">
                            <AppTextField
                                name="title"
                                label={t('title') || 'Title'}
                                placeholder={t('typeYourTitle')}
                            />
                        </div>

                        <Form.Item label={t('status')}>
                            <Controller
                                name="statuses"
                                control={control}
                                render={({ field }) => (
                                    <Select {...field} showSearch filterOption={defaultSelectFilter} style={{ width: '100%' }} options={statusOptions} placeholder={t('selectStatus')} mode="multiple" />
                                )}
                            />
                        </Form.Item>

                        <Form.Item label={t('currentStep')}>
                            <Controller
                                name="stepIds"
                                control={control}
                                render={({ field }) => (
                                    <Select {...field} showSearch filterOption={defaultSelectFilter} style={{ width: '100%' }} placeholder={t('selectSteps')} mode="multiple" />
                                )}
                            />
                        </Form.Item>

                        <Form.Item label={t('provider')}>
                            <Controller
                                name="providerIds"
                                control={control}
                                render={({ field }) => (
                                    <Select
                                        {...field}
                                        showSearch
                                        filterOption={defaultSelectFilter}
                                        style={{ width: '100%' }}
                                        placeholder={t('selectProvider')}
                                        options={providersData}
                                        mode="multiple"
                                    />
                                )}
                            />
                        </Form.Item>

                        <Form.Item label={t('client')}>
                            <Controller
                                name="clientIds"
                                control={control}
                                render={({ field }) => (
                                    <Select
                                        {...field}
                                        showSearch
                                        filterOption={defaultSelectFilter}
                                        style={{ width: '100%' }}
                                        placeholder={t('selectClient')}
                                        options={clientsData}
                                        mode="multiple"
                                    />
                                )}
                            />
                        </Form.Item>
                    </Form>
                </form>
            </FormProvider>
        </AppModalComponent>
    );
}
