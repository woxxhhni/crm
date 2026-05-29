'use client';

import { Form, Input, DatePicker, App } from 'antd';
import { useState, useEffect, useCallback } from 'react';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm, Controller, SubmitHandler } from 'react-hook-form';
import AppModal from '@/components/modal/app-modal.component';
import AppUploadDraggerComponent from '@/components/form/upload/app-upload-dragger.component';
import { container } from 'tsyringe';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useTranslate } from '@/locales/use-locales';
import { OrdersService } from '@/features/orders/services/orders.service';
import dayjs from 'dayjs';
import { formatDateForAPI } from '@/utils/date-utils';

const createSchema = (t: (key: string, params?: Record<string, string>) => string) =>
  z.object({
    description: z.string().optional(),
    actionDate: z.any().refine((val) => val && dayjs(val).isValid(), { message: t('fieldIsRequired', { field: t('date') }) }),
  });

export type OrderActionForm = z.infer<ReturnType<typeof createSchema>>;

interface Props {
  open: boolean;
  onClose: () => void;
  orderId: string | number;
  actionType: 'complete' | 'cancel' | 'suspend' | 'unsuspend' | null;
}

export default function OrderActionModalComponent({ open, onClose, orderId, actionType }: Props) {
  const [fileList, setFileList] = useState<any[]>([]);
  const { t } = useTranslate('orders');
  const { notification } = App.useApp();
  const queryClient = useQueryClient();
  const service = container.resolve(OrdersService);

  const schema = createSchema(t);

  const {
    control,
    reset,
    handleSubmit,
    formState: { errors },
  } = useForm<OrderActionForm>({
    resolver: zodResolver(schema),
    defaultValues: {
      description: '',
      actionDate: dayjs(),
    },
  });

  useEffect(() => {
    if (!open) {
      reset({
        description: '',
        actionDate: dayjs(),
      });
      setFileList([]);
    }
  }, [open, reset]);

  const mutation = useMutation({
    mutationFn: async (data: OrderActionForm) => {
      if (!actionType) return;

      const files = fileList.filter((f) => f.originFileObj).map((f) => f.originFileObj as File);

      const formData = new FormData();
      formData.append('Description', data.description || '');
      formData.append('ActionDate', formatDateForAPI(data.actionDate));

      files.forEach((file) => {
        formData.append('Files', file);
      });

      switch (actionType) {
        case 'complete':
          await service.completeOrder(orderId, formData);
          break;
        case 'cancel':
          await service.cancelOrder(orderId, formData);
          break;
        case 'suspend':
          await service.suspendOrder(orderId, formData);
          break;
        case 'unsuspend':
          await service.unsuspendOrder(orderId, formData);
          break;
      }
    },
    onSuccess: async () => {
      const actionMessages = {
        complete: t('orderCompletedSuccessfully'),
        cancel: t('orderCanceledSuccessfully'),
        suspend: t('orderSuspendedSuccessfully'),
        unsuspend: t('orderUnsuspendedSuccessfully'),
      };
      notification.success({ message: actionMessages[actionType!] || t('orderActionSuccessful') });
      onClose();
      // Invalidate only the specific order details query
      // This will refresh both the header and details page since they use the same query key
      await queryClient.invalidateQueries({
        queryKey: ['order-edit-details', String(orderId)],
        exact: true,
      });
    },
  });

  const handleSubmitForm: SubmitHandler<OrderActionForm> = async (data) => {
    await mutation.mutateAsync(data);
  };

  const handleUploadChange = useCallback(({ fileList: newFileList }: any) => {
    setFileList(newFileList);
  }, []);

  const getModalTitle = () => {
    const titleMap = {
      complete: t('completeOrder'),
      cancel: t('cancelOrder'),
      suspend: t('suspendOrder'),
      unsuspend: t('unsuspendOrder'),
    };
    return titleMap[actionType!] || t('orderAction');
  };

  const getSubmitText = () => {
    return t('save');
  };

  if (!actionType) return null;

  return (
    <AppModal open={open} onClose={onClose} title={getModalTitle()} onSubmit={handleSubmit(handleSubmitForm)} submitText={getSubmitText()} width={648} loading={mutation.isPending}>
      <Form layout='vertical'>
        <Form.Item label={t('date')} required validateStatus={errors.actionDate ? 'error' : ''} help={errors.actionDate?.message as string}>
          <Controller
            name='actionDate'
            control={control}
            render={({ field }) => <DatePicker {...field} className='w-full' placeholder={t('selectDate')} status={errors.actionDate ? 'error' : ''} />}
          />
        </Form.Item>

        <Form.Item label={t('descriptionOptional')}>
          <Controller
            name='description'
            control={control}
            render={({ field }) => <Input.TextArea {...field} placeholder={t('writeDescription')} maxLength={500} showCount rows={4} />}
          />
        </Form.Item>

        <Form.Item label={t('attachmentsOptional')}>
          <AppUploadDraggerComponent fileList={fileList} onChange={handleUploadChange} />
        </Form.Item>
      </Form>
    </AppModal>
  );
}
