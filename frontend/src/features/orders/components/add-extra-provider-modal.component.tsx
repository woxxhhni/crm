import { Modal, Form, Select, InputNumber, notification, Button } from 'antd';
import { useMutation, useQueryClient, useQuery } from '@tanstack/react-query';
import { container } from 'tsyringe';
import { OrdersService } from '@/features/orders/services/orders.service';
import { useTranslate } from '@/locales/use-locales';
import { ExtraProviderCreateDTO } from '@/api/orders/types';
import { defaultSelectFilter } from '@/utils/select-utils';

interface Props {
    open: boolean;
    onCancel: () => void;
    orderId: string | number;
}

export default function AddExtraProviderModal({ open, onCancel, orderId }: Props) {
    const { t } = useTranslate('orders');
    const [form] = Form.useForm();
    const service = container.resolve(OrdersService);
    const queryClient = useQueryClient();

    const { data: providerOptions, isLoading: providersLoading } = useQuery({
        queryKey: ['provider-options'],
        queryFn: () => service.providerOptions(),
        staleTime: Infinity,
    });

    const { data: currencyOptions, isLoading: currenciesLoading } = useQuery({
        queryKey: ['currency-options'],
        queryFn: () => service.currenciesOptions(),
        staleTime: Infinity,
    });

    const mutation = useMutation({
        mutationFn: (values: ExtraProviderCreateDTO) => service.addExtraProvider(orderId, values),
        onSuccess: () => {
            notification.success({ message: t('extraProviderAddedSuccessfully') });
            queryClient.invalidateQueries({ queryKey: ['order-edit-details', orderId] });
            form.resetFields();
            onCancel();
        },
        onError: () => {
            notification.error({ message: t('failedToAddExtraProvider') });
        }
    });

    const handleSubmit = async () => {
        try {
            const values = await form.validateFields();
            mutation.mutate(values);
        } catch (error) {
            // Form validation error
        }
    };

    return (
        <Modal
            open={open}
            title={t('addExtraProvider')}
            onCancel={onCancel}
            onOk={handleSubmit}
            confirmLoading={mutation.isPending}
            okText={t('add')}
            cancelText={t('cancel')}
        >
            <Form form={form} layout="vertical">
                <Form.Item
                    name="providerId"
                    label={t('provider')}
                    rules={[{ required: true, message: t('pleaseSelectProvider') }]}
                >
                    <Select
                        showSearch
                        placeholder={t('selectProvider')}
                        filterOption={defaultSelectFilter}
                        options={providerOptions}
                        loading={providersLoading}
                    />
                </Form.Item>

                <Form.Item
                    label={t('amount')}
                    required
                    className='mb-0'
                >
                    <div className='flex gap-2'>
                        <Form.Item
                            name="amount"
                            rules={[{ required: true, message: t('pleaseEnterAmount') }]}
                            className='flex-1 mb-0'
                        >
                            <InputNumber
                                style={{ width: '100%' }}
                                placeholder={t('amount')}
                                min={0}
                                precision={2}
                            />
                        </Form.Item>
                        <Form.Item
                            name="currency"
                            rules={[{ required: true, message: t('pleaseSelectCurrency') }]}
                            className='w-[120px] mb-0'
                            initialValue="USD"
                        >
                            <Select
                                showSearch
                                placeholder={t('currency')}
                                options={currencyOptions}
                                loading={currenciesLoading}
                            />
                        </Form.Item>
                    </div>
                </Form.Item>
            </Form>
        </Modal>
    );
}
