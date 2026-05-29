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
    actionDate: z.any().refine((val) => val, { message: t('fieldIsRequired', { field: t('date') }) }),
    title: z.string().min(1, t('fieldIsRequired', { field: t('title') })),
    description: z.string().min(1, t('fieldIsRequired', { field: t('description') })),
  });

export type NoteForm = z.infer<ReturnType<typeof createSchema>>;

interface Props {
  open: boolean;
  onClose: () => void;
  orderId: string | number;
}

export default function AddNoteModalComponent({ open, onClose, orderId }: Props) {
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
  } = useForm<NoteForm>({
    resolver: zodResolver(schema),
    defaultValues: {
      actionDate: dayjs(),
      title: '',
      description: '',
    },
  });

  useEffect(() => {
    if (!open) {
      reset({
        actionDate: dayjs(),
        title: '',
        description: '',
      });
      setFileList([]);
    }
  }, [open, reset]);

  const createMutation = useMutation({
    mutationFn: async (data: NoteForm) => {
      const files = fileList.filter((f) => f.originFileObj).map((f) => f.originFileObj as File);

      await service.createNote(orderId, {
        actionDate: formatDateForAPI(data.actionDate),
        title: data.title,
        description: data.description,
        files,
      });
    },
    onSuccess: async () => {
      notification.success({ message: t('noteCreatedSuccessfully') });
      await queryClient.invalidateQueries({ queryKey: ['order-edit-details', String(orderId)] });
      onClose();
    },
  });

  const handleSubmitForm: SubmitHandler<NoteForm> = async (data) => {
    await createMutation.mutateAsync(data);
  };

  const handleUploadChange = useCallback(({ fileList: newFileList }: any) => {
    setFileList(newFileList);
  }, []);

  return (
    <AppModal open={open} onClose={onClose} title={t('addNote')} onSubmit={handleSubmit(handleSubmitForm)} submitText={t('add')} width={648} loading={createMutation.isPending}>
      <Form layout='vertical'>
        <Form.Item label={t('date')} required validateStatus={errors.actionDate ? 'error' : ''} help={errors.actionDate?.message as string}>
          <Controller
            name='actionDate'
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

        <Form.Item label={t('title')} required validateStatus={errors.title ? 'error' : ''} help={errors.title?.message}>
          <Controller name='title' control={control} render={({ field }) => <Input {...field} placeholder={t('writeTitle')} status={errors.title ? 'error' : ''} />} />
        </Form.Item>

        <Form.Item label={t('description')} required validateStatus={errors.description ? 'error' : ''} help={errors.description?.message}>
          <Controller
            name='description'
            control={control}
            render={({ field }) => <Input.TextArea {...field} placeholder={t('writeDescription')} maxLength={500} showCount rows={4} status={errors.description ? 'error' : ''} />}
          />
        </Form.Item>

        <Form.Item label={t('attachments')}>
          <AppUploadDraggerComponent fileList={fileList} onChange={handleUploadChange} />
        </Form.Item>
      </Form>
    </AppModal>
  );
}
