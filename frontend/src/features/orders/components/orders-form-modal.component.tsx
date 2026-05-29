'use client';

import { App, Form, DatePicker, Select, Row, Col, Divider, Typography, Input } from 'antd';
import { AppInput } from '@/components/form/amount-input';
import { useState, useEffect } from 'react';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm, Controller, SubmitHandler } from 'react-hook-form';
import AppModal from '@/components/modal/app-modal.component';
import AppFileUploadComponent from '@/components/form/upload/app-file-upload.component';
import { container } from 'tsyringe';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useTranslate } from '@/locales/use-locales';
import { OrdersService } from '@/features/orders/services/orders.service';
import type { OrderServiceDTO } from '@/features/orders/dto/orders.service.dto';
import dayjs from 'dayjs';
import { defaultSelectFilter } from '@/utils/select-utils';
import { formatDateForAPI } from '@/utils/date-utils';

const createSchema = (t: (key: string, params?: Record<string, string>) => string) =>
  z.object({
    OrderDate: z.any().refine((val) => val, { message: t('fieldIsRequired', { field: t('OrderDate') }) }),
    title: z
      .string({ message: t('fieldIsRequired', { field: t('title') }) })
      .min(1, t('fieldIsRequired', { field: t('title') })),
    description: z.string().optional(),
    provider: z.number({ message: t('fieldIsRequired', { field: t('provider') }) }),
    buyCurrency: z
      .string({ message: t('fieldIsRequired', { field: t('buyCurrency') }) })
      .min(1, t('fieldIsRequired', { field: t('buyCurrency') })),
    buyAmount: z.union([z.string(), z.number()]).optional(),
    sellAmount: z.union([z.string(), z.number()]).optional(),
    customer: z.number({ message: t('fieldIsRequired', { field: t('customer') }) }),
    sellCurrency: z
      .string({ message: t('fieldIsRequired', { field: t('sellCurrency') }) })
      .min(1, t('fieldIsRequired', { field: t('sellCurrency') })),
    userIds: z.array(z.union([z.string(), z.number()])).optional(),
    buyInvoice: z.any().optional(),
    sellInvoice: z.any().optional(),
  });

export type OrderForm = z.infer<ReturnType<typeof createSchema>>;

interface Props {
  open: boolean;
  onClose: () => void;
  initialValues?: { id?: number | string } | null;
  isCreateForm?: boolean;
}

export default function OrdersFormModalComponent({ open, onClose, initialValues, isCreateForm = false }: Props) {
  const [buyInvoiceFiles, setBuyInvoiceFiles] = useState<any[]>([]);
  const [sellInvoiceFiles, setSellInvoiceFiles] = useState<any[]>([]);
  const [originalBuyFileIds, setOriginalBuyFileIds] = useState<number[]>([]);
  const [originalSellFileIds, setOriginalSellFileIds] = useState<number[]>([]);
  const { t } = useTranslate('orders');
  const { notification } = App.useApp();
  const queryClient = useQueryClient();
  const service = container.resolve(OrdersService);

  const schema = createSchema(t);

  const {
    control,
    reset,
    setValue,
    formState: { errors },
    handleSubmit,
  } = useForm<OrderForm>({
    resolver: zodResolver(schema),
    defaultValues: {},
  });

  const { data: orderDetails, isLoading: isDetailsLoading } = useQuery({
    queryKey: ['order-edit-details', initialValues?.id],
    queryFn: () => service.getDetails(initialValues!.id!),
    enabled: !!initialValues?.id && !isCreateForm,
  });

  useEffect(() => {
    if (orderDetails && !isCreateForm) {
      // Extract date part without timezone conversion
      // Backend sends: "2026-01-01T00:00:00" or "2026-01-01T00:00:00Z"
      // We extract: "2026-01-01"
      const orderDateValue = orderDetails.orderDate ? orderDetails.orderDate.split('T')[0] : null;

      reset({
        OrderDate: orderDateValue,
        title: orderDetails.title ?? '',
        description: orderDetails.description ?? '',
        provider: orderDetails.providerId ?? undefined,
        buyCurrency: orderDetails.buyCurrency ?? '',
        buyAmount: orderDetails.buyAmount?.toString() ?? '',
        sellAmount: orderDetails.sellAmount?.toString() ?? '',
        customer: orderDetails.clientId ?? undefined,
        sellCurrency: orderDetails.sellCurrency ?? '',
        userIds: orderDetails.employees?.map((emp: any) => emp.employeeId) ?? [],
        buyInvoice: [],
        sellInvoice: [],
      });

      // Store file info for display
      const buyFiles = (orderDetails.buyInvoiceLinks ?? []).map((link: any) => ({
        uid: String(link.fileId),
        name: link.fileName,
        status: 'done' as const,
        url: link.url,
      }));
      const sellFiles = (orderDetails.sellInvoiceLinks ?? []).map((link: any) => ({
        uid: String(link.fileId),
        name: link.fileName,
        status: 'done' as const,
        url: link.url,
      }));

      // Store original file IDs for tracking removals
      const buyFileIds = (orderDetails.buyInvoiceLinks ?? []).map((link: any) => link.fileId);
      const sellFileIds = (orderDetails.sellInvoiceLinks ?? []).map((link: any) => link.fileId);

      setBuyInvoiceFiles(buyFiles);
      setSellInvoiceFiles(sellFiles);
      setOriginalBuyFileIds(buyFileIds);
      setOriginalSellFileIds(sellFileIds);
    }
  }, [orderDetails, isCreateForm, reset, setValue]);

  const { data: providersData } = useQuery({
    queryKey: ['providers-option-list'],
    queryFn: () => service.providerOptions(),
  });
  const { data: clientsData } = useQuery({
    queryKey: ['clients-option-list'],
    queryFn: () => service.clientOptions(),
  });
  const { data: currenciesData } = useQuery({
    queryKey: ['currencies-option-list'],
    queryFn: () => service.currenciesOptions(),
  });
  const { data: employeesData } = useQuery({
    queryKey: ['employees-option-list'],
    queryFn: () => service.employeeOptions(),
  });

  const createMutation = useMutation({
    mutationFn: async (data: OrderForm) => {
      const dto: OrderServiceDTO = {
        OrderDate: formatDateForAPI(data.OrderDate),
        title: data.title,
        description: data.description,
        providerId: data.provider,
        clientId: data.customer,
        buyCurrency: data.buyCurrency,
        buyAmount: data.buyAmount,
        sellCurrency: data.sellCurrency,
        sellAmount: data.sellAmount,
        assignedByUserId: data.userIds?.map(Number) ?? [],
        buyInvoiceFiles: data.buyInvoice,
        sellInvoiceFiles: data.sellInvoice,
      };
      await service.create(dto);
    },
    onSuccess: async () => {
      notification.success({ message: t('orderCreatedSuccessfully') });
      await queryClient.invalidateQueries({ queryKey: ['orders-list'] });
      onClose();
    },
  });

  const updateMutation = useMutation({
    mutationFn: async (data: OrderForm) => {
      if (!initialValues?.id) throw new Error('Missing order ID');

      // Current file IDs (existing files that are still in the list)
      const currentBuyFileIds = buyInvoiceFiles.filter((f) => !f.originFileObj && f.url).map((f) => Number(f.uid));
      const currentSellFileIds = sellInvoiceFiles.filter((f) => !f.originFileObj && f.url).map((f) => Number(f.uid));

      // Removed file IDs (files that were in original but not in current)
      const removedBuyFileIds = originalBuyFileIds.filter((id) => !currentBuyFileIds.includes(id));
      const removedSellFileIds = originalSellFileIds.filter((id) => !currentSellFileIds.includes(id));

      // Build files array: existing file IDs and new File objects
      const buyFiles: (File | number)[] = [...currentBuyFileIds, ...buyInvoiceFiles.filter((f) => f.originFileObj).map((f) => f.originFileObj)];
      const sellFiles: (File | number)[] = [...currentSellFileIds, ...sellInvoiceFiles.filter((f) => f.originFileObj).map((f) => f.originFileObj)];

      const dto: OrderServiceDTO = {
        OrderDate: formatDateForAPI(data.OrderDate),
        title: data.title,
        description: data.description,
        providerId: data.provider,
        clientId: data.customer,
        buyCurrency: data.buyCurrency,
        buyAmount: data.buyAmount,
        sellCurrency: data.sellCurrency,
        sellAmount: data.sellAmount,
        assignedByUserId: data.userIds?.map(Number) ?? [],
        buyInvoiceFiles: buyFiles,
        sellInvoiceFiles: sellFiles,
        removedBuyFileIds: removedBuyFileIds,
        removedSellFileIds: removedSellFileIds,
      };

      await service.update(initialValues.id!, dto);
    },
    onSuccess: async () => {
      notification.success({ message: t('orderUpdatedSuccessfully') });
      await queryClient.invalidateQueries({ queryKey: ['orders-list'] });
      onClose();
    },
  });

  const handleSubmitForm: SubmitHandler<OrderForm> = async (data) => {
    if (isCreateForm) await createMutation.mutateAsync(data);
    else await updateMutation.mutateAsync(data);
  };

  const handleBuyUploadChange = ({ fileList }: any) => {
    setBuyInvoiceFiles(fileList);
    setValue('buyInvoice', fileList.map((f: any) => f.originFileObj).filter(Boolean));
  };
  const handleSellUploadChange = ({ fileList }: any) => {
    setSellInvoiceFiles(fileList);
    setValue('sellInvoice', fileList.map((f: any) => f.originFileObj).filter(Boolean));
  };

  // Check if order is in progress - only allow employee assignment for in-progress orders
  const isOrderInProgress = isCreateForm || orderDetails?.status?.toLowerCase() === 'inprogress';

  return (
    <AppModal
      open={open}
      onClose={onClose}
      title={isCreateForm ? t('addOrder') : t('editOrder')}
      onSubmit={handleSubmit(handleSubmitForm)}
      submitText={t('save')}
      width={650}
      loading={createMutation.isPending || updateMutation.isPending || (!isCreateForm && isDetailsLoading)}>
      <Form layout='vertical'>
        <Typography className='font-semibold'>{t('orderBasicInformation')}</Typography>
        <Divider style={{ marginTop: 15 }} />

        <Form.Item label={t('OrderDate')} required validateStatus={errors.OrderDate ? 'error' : ''} help={errors.OrderDate?.message as string}>
          <Controller
            name='OrderDate'
            control={control}
            render={({ field }) => (
              <DatePicker
                format='YYYY-MM-DD'
                value={field.value ? dayjs(field.value, 'YYYY-MM-DD') : null}
                onChange={(val) => field.onChange(val ? val.format('YYYY-MM-DD') : null)}
                style={{ width: '100%' }}
                status={errors.OrderDate ? 'error' : ''}
              />
            )}
          />
        </Form.Item>

        <Form.Item label={t('title')} required validateStatus={errors.title ? 'error' : ''} help={errors.title?.message}>
          <Controller name='title' control={control} render={({ field }) => <Input {...field} status={errors.title ? 'error' : ''} />} />
        </Form.Item>

        <Form.Item label={t('description')}>
          <Controller name='description' control={control} render={({ field }) => <Input.TextArea {...field} />} />
        </Form.Item>

        <Typography className='font-semibold mt-5'>{t('providerDetails')}</Typography>
        <Divider style={{ marginTop: 15 }} />

        <Row gutter={16}>
          <Col span={12}>
            <Form.Item label={t('provider')} required validateStatus={errors.provider ? 'error' : ''} help={errors.provider?.message}>
              <Controller
                name='provider'
                control={control}
                render={({ field }) => (
                  <Select {...field} showSearch filterOption={defaultSelectFilter} options={providersData} placeholder='Select provider' status={errors.provider ? 'error' : ''} />
                )}
              />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item label={t('buyCurrency')} required validateStatus={errors.buyCurrency ? 'error' : ''} help={errors.buyCurrency?.message}>
              <Controller
                name='buyCurrency'
                control={control}
                render={({ field }) => <Select {...field} showSearch filterOption={defaultSelectFilter} options={currenciesData} status={errors.buyCurrency ? 'error' : ''} />}
              />
            </Form.Item>
          </Col>
        </Row>

        <Row gutter={16}>
          <Col span={12}>
            <Form.Item label={t('buyAmount')}>
              <Controller name='buyAmount' control={control} render={({ field }) => <AppInput {...field} number separate placeholder={t('enterAmount')} />} />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item label={t('buyerInvoice')}>
              <AppFileUploadComponent fileList={buyInvoiceFiles} onChange={handleBuyUploadChange} />
            </Form.Item>
          </Col>
        </Row>

        <Typography className='font-semibold mt-5'>{t('customerDetails')}</Typography>
        <Divider style={{ marginTop: 15 }} />

        <Row gutter={16}>
          <Col span={12}>
            <Form.Item label={t('customer')} required validateStatus={errors.customer ? 'error' : ''} help={errors.customer?.message}>
              <Controller
                name='customer'
                control={control}
                render={({ field }) => <Select {...field} showSearch filterOption={defaultSelectFilter} options={clientsData} placeholder='Select customer' status={errors.customer ? 'error' : ''} />}
              />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item label={t('sellCurrency')} required validateStatus={errors.sellCurrency ? 'error' : ''} help={errors.sellCurrency?.message}>
              <Controller
                name='sellCurrency'
                control={control}
                render={({ field }) => <Select {...field} showSearch filterOption={defaultSelectFilter} options={currenciesData} status={errors.sellCurrency ? 'error' : ''} />}
              />
            </Form.Item>
          </Col>
        </Row>

        <Row gutter={16}>
          <Col span={12}>
            <Form.Item label={t('sellAmount')}>
              <Controller name='sellAmount' control={control} render={({ field }) => <AppInput {...field} number separate placeholder={t('enterAmount')} />} />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item label={t('sellerInvoice')}>
              <AppFileUploadComponent fileList={sellInvoiceFiles} onChange={handleSellUploadChange} />
            </Form.Item>
          </Col>
        </Row>

        <Typography className='font-semibold mt-5'>{t('assignedEmployees')}</Typography>
        <Divider style={{ marginTop: 15 }} />

        <Form.Item label={t('employees')}>
          <Controller
            name='userIds'
            control={control}
            render={({ field }) => (
              <Select
                {...field}
                showSearch
                filterOption={defaultSelectFilter}
                mode='multiple'
                options={employeesData}
                placeholder='Select employees'
                onChange={(values) => field.onChange(values.map(Number))}
                disabled={!isOrderInProgress}
              />
            )}
          />
        </Form.Item>
      </Form>
    </AppModal>
  );
}
