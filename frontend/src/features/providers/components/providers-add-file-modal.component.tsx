'use client';

import { App, Form, Input } from 'antd';
import { useState, useEffect, useMemo, useCallback, useRef } from 'react';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm, Controller } from 'react-hook-form';
import AppModal from '@/components/modal/app-modal.component';
import AppUploadDraggerComponent from '@/components/form/upload/app-upload-dragger.component';
import { container } from 'tsyringe';
import { ProvidersService } from '@/features/providers/services/providers.service';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useParams } from 'next/navigation';
import { useTranslate } from '@/locales/use-locales';
import { usePermissionContext, PERMISSIONS } from '@/features/permissions';

const createSchema = (t: (key: string, options?: Record<string, unknown>) => string) => z.object({
  title: z.string().min(1, t('fieldIsRequired', { field: t('title') })),
  files: z.any().optional(),
});

type FileSetForm = z.infer<ReturnType<typeof createSchema>>;

interface Props {
  open: boolean;
  onClose: () => void;
  mode: 'add' | 'editTitle' | 'editFiles';
  initialValues?: {
    title?: string;
    id?: number | string;
    files?: any[];
  };
}

export default function ProvidersAddFileModalComponent({ open, onClose, mode, initialValues }: Props) {
  const [fileList, setFileList] = useState<any[]>([]);
  const initialValuesRef = useRef(initialValues);
  const queryClient = useQueryClient();
  const { t } = useTranslate('providers');
  const schema = useMemo(() => createSchema(t), [t]);
  const params = useParams();
  const providerId = Number(params.id);
  const { notification } = App.useApp();
  const { hasPermission } = usePermissionContext();

  const canDeleteFile = hasPermission(PERMISSIONS.FILE_GROUP_DELETE);
  const service = useMemo(() => container.resolve(ProvidersService), []);

  const {
    control,
    getValues,
    reset,
    setValue,
    handleSubmit: handleFormSubmit,
    formState: { errors },
  } = useForm<FileSetForm>({
    resolver: zodResolver(schema),
    defaultValues: { title: '', files: [] },
  });

  const { mutateAsync, isPending } = useMutation({
    mutationFn: async () => {
      const currentId = initialValuesRef.current?.id;
      const groupId = typeof currentId === 'string' ? Number(currentId) : currentId;
      const files = getValues('files') ?? [];

      if (mode === 'editTitle' && groupId) {
        await service.updateFileGroupLabel(providerId, groupId, getValues('title') ?? '');
      } else if (mode === 'editFiles' && groupId) {
        // Only call API if there are new files to upload
        if (files.length > 0) {
          await service.addFilesToGroup(providerId, groupId, files);
        }
      } else {
        await service.createFileGroup(providerId, {
          label: getValues('title') ?? '',
          files: files,
        });
      }
    },
    onSuccess: async () => {
      notification.success({
        message: mode === 'editTitle' ? t('fileTitleUpdatedSuccessfully') : mode === 'editFiles' ? t('filesAddedSuccessfully') : t('fileCreatedSuccessfully'),
      });
      await queryClient.invalidateQueries({ queryKey: ['provider-file-groups', providerId] });
      onClose();
    },
  });

  const title = useMemo(() => (mode === 'add' ? t('addFileSet') : mode === 'editTitle' ? t('editTitle') : t('editFiles')), [mode, t]);

  const submitText = useMemo(() => (mode === 'editTitle' || mode === 'editFiles' ? t('saveChanges') : t('add')), [mode, t]);

  // Only reset form when modal opens with new initialValues
  useEffect(() => {
    if (open) {
      initialValuesRef.current = initialValues;
      reset({ title: initialValues?.title || '', files: [] });
      setFileList(initialValues?.files || []);
    }
  }, [open]); // Only depend on open, not initialValues

  // Clear form when modal closes
  useEffect(() => {
    if (!open) {
      reset({ title: '', files: [] });
      setFileList([]);
    }
  }, [open, reset]);

  const handleUploadChange = useCallback(
    ({ fileList: newFileList }: any) => {
      setFileList(newFileList);

      const realFiles = newFileList.map((f: any) => (f.originFileObj instanceof File ? f.originFileObj : null)).filter(Boolean);

      setValue('files', realFiles);
    },
    [setValue]
  );

  const handleDeleteExistingFile = useCallback(
    async (file: any) => {
      try {
        const fileId = file.uid ? Number(file.uid) : null;
        if (fileId) {
          await service.deleteFileFromGroup(providerId, fileId);
          notification.success({ message: t('fileDeletedSuccessfully') });
          await queryClient.invalidateQueries({ queryKey: ['provider-file-groups', providerId] });
        }
      } catch (error) {
        console.error(error);
      }
    },
    [providerId, notification, queryClient, service, t]
  );

  const handleSubmit = useCallback(
    handleFormSubmit(async () => {
      try {
        await mutateAsync();
      } catch (err) {
        console.error(err);
      }
    }),
    [handleFormSubmit, mutateAsync]
  );

  return (
    <AppModal open={open} onClose={onClose} title={title} onSubmit={handleSubmit} submitText={submitText} width={648} loading={isPending}>
      <Form layout='vertical' onFinish={handleSubmit}>
        {(mode === 'add' || mode === 'editTitle') && (
          <Form.Item validateStatus={errors.title ? 'error' : ''} help={errors.title?.message} label={t('title')}>
            <Controller name='title' control={control} render={({ field }) => <Input {...field} placeholder={t('enterTitle')} />} />
          </Form.Item>
        )}

        {(mode === 'add' || mode === 'editFiles') && (
          <AppUploadDraggerComponent
            fileList={fileList}
            onChange={handleUploadChange}
            showDeleteConfirm={mode === 'editFiles'}
            onDeleteExistingFile={handleDeleteExistingFile}
            canDelete={canDeleteFile}
          />
        )}
      </Form>
    </AppModal>
  );
}
