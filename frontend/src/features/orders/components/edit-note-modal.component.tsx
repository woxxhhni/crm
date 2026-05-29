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
import type { NoteDetailResponse } from '@/api/orders/types';
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
  noteId: string | number;
  noteDetail?: NoteDetailResponse;
}

export default function EditNoteModalComponent({ open, onClose, orderId, noteId, noteDetail }: Props) {
  const [fileList, setFileList] = useState<any[]>([]);
  const [removedFileIds, setRemovedFileIds] = useState<number[]>([]);
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
    if (open && noteDetail) {
      // Extract date part without timezone conversion
      // Backend sends: "2025-12-02T20:30:00" or "2025-12-02T20:30:00Z"
      // We extract: "2025-12-02" and let DatePicker handle it
      const actionDateValue = noteDetail.date ? noteDetail.date.split('T')[0] : dayjs().format('YYYY-MM-DD');

      // Set form values from noteDetail
      reset({
        actionDate: actionDateValue,
        title: noteDetail.title || '',
        description: noteDetail.description || '',
      });

      // Set existing files
      if (noteDetail.files && noteDetail.files.length > 0) {
        const existingFiles = noteDetail.files
          .filter((f) => f.url)
          .map((f) => ({
            uid: String(f.id),
            name: f.name || 'File',
            status: 'done',
            url: f.url,
            existingId: f.id,
          }));
        setFileList(existingFiles);
      } else {
        setFileList([]);
      }
      setRemovedFileIds([]);
    }
  }, [open, noteDetail, reset]);

  const updateMutation = useMutation({
    mutationFn: async (data: NoteForm) => {
      const newFiles = fileList.filter((f) => f.originFileObj).map((f) => f.originFileObj as File);

      await service.updateNote(orderId, noteId, {
        actionDate: formatDateForAPI(data.actionDate),
        title: data.title,
        description: data.description,
        files: newFiles,
        removedFileIds,
      });
    },
    onSuccess: async () => {
      notification.success({ message: t('noteUpdatedSuccessfully') });
      await queryClient.invalidateQueries({ queryKey: ['order-edit-details', String(orderId)] });
      await queryClient.invalidateQueries({ queryKey: ['note-detail', String(orderId), noteId] });
      onClose();
    },
  });

  const handleSubmitForm: SubmitHandler<NoteForm> = async (data) => {
    await updateMutation.mutateAsync(data);
  };

  const handleUploadChange = useCallback(({ fileList: newFileList }: any) => {
    setFileList(newFileList);
  }, []);

  const handleRemoveFile = useCallback((file: any) => {
    if (file.existingId) {
      setRemovedFileIds((prev) => [...prev, file.existingId]);
    }
    return true;
  }, []);

  return (
    <AppModal
      open={open}
      onClose={onClose}
      title={t('editNote')}
      onSubmit={handleSubmit(handleSubmitForm)}
      submitText={t('save')}
      width={648}
      loading={updateMutation.isPending}>
      <Form layout='vertical'>
        <Form.Item label={t('date')} required validateStatus={errors.actionDate ? 'error' : ''} help={errors.actionDate?.message as string}>
          <Controller
            name='actionDate'
            control={control}
            render={({ field }) => (
              <DatePicker
                {...field}
                format='YYYY-MM-DD'
                value={field.value ? dayjs(field.value, 'YYYY-MM-DD') : null}
                onChange={(date) => field.onChange(date ? date.format('YYYY-MM-DD') : null)}
                style={{ width: '100%' }}
                placeholder={t('selectDate')}
                status={errors.actionDate ? 'error' : ''}
              />
            )}
          />
        </Form.Item>

        <Form.Item label={t('title')} required validateStatus={errors.title ? 'error' : ''} help={errors.title?.message}>
          <Controller
            name='title'
            control={control}
            render={({ field }) => <Input {...field} placeholder={t('writeTitle')} status={errors.title ? 'error' : ''} />}
          />
        </Form.Item>

        <Form.Item label={t('description')} required validateStatus={errors.description ? 'error' : ''} help={errors.description?.message}>
          <Controller
            name='description'
            control={control}
            render={({ field }) => (
              <Input.TextArea {...field} placeholder={t('writeDescription')} maxLength={500} showCount rows={4} status={errors.description ? 'error' : ''} />
            )}
          />
        </Form.Item>

        <Form.Item label={t('attachments')}>
          <AppUploadDraggerComponent fileList={fileList} onChange={handleUploadChange} onRemove={handleRemoveFile} />
        </Form.Item>
      </Form>
    </AppModal>
  );
}
